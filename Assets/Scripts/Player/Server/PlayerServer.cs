using UnityEngine;
using System.Collections.Generic;
using Bangboom.StateMachine;
using Photon.Pun;

namespace Bangboom.Player.Server
{
    public class PlayerServer : MonoBehaviourPun
    {
        #region ClassVariables

        [SerializeField] private MovementStateMachine movementStateMachine;

        public uint serverSnapshotRate;
        private uint serverTickNumber;
        private uint serverTickAccumulator;
        private Queue<PlayerSyncStructs.InputMessage> serverInputMessages;
        private Rigidbody2D proxyClientPlayer;

        #endregion

        #region Initialization

        private void Awake()
        {
            serverTickNumber = 0;
            serverTickAccumulator = 0;
            serverInputMessages = new Queue<PlayerSyncStructs.InputMessage>();
            Application.targetFrameRate = 100; 
        }

        private void Start()
        {
            if (photonView.IsMine)
            {
                serverSnapshotRate = GetServerSnapshotRate();
            }
        }

        public void SetProxyClientPlayer(Rigidbody2D proxyPlayer)
        {
            proxyClientPlayer = proxyPlayer;
        }
        
        /// <summary>
        /// Gets the server rate based on frames
        /// </summary>
        private static uint GetServerSnapshotRate()
        {
            var deltaTimeFPS = 0f;
            deltaTimeFPS += Time.deltaTime;
            deltaTimeFPS /= 2.0f;
            var fps = (int) (1.0f / deltaTimeFPS);
            return (uint) (fps / 20);
        }

        #endregion

        private void Update()
        {
            ServerUpdate();
        }

        #region ServerUpdate
        
        /// <summary>
        /// Sends updated if host of this player / perform inputs if on another player
        /// </summary>
        private void ServerUpdate()
        {
            if(!photonView.IsMine)
            {
                movementStateMachine.CanUpdate = false;
            }
            var tempServerTickNumber = serverTickNumber;
            var tempServerTickAccumulator = serverTickAccumulator;

            while (serverInputMessages.Count > 0 && (float) PhotonNetwork.Time >= serverInputMessages.Peek().DeliveryTime)
            {
                var inputMsg = serverInputMessages.Dequeue();

                // message contains an array of inputs, calculate what tick the final one is
                var maxTick = inputMsg.StartTickNumber + (uint) inputMsg.Inputs.Count - 1;

                // if that tick is greater than or equal to the current tick we're on, then it has inputs which are new
                if (maxTick < tempServerTickNumber)
                {
                    continue;
                }
                
                // there may be some inputs in the array that we've already had, so figure out where to start
                var startIndex = tempServerTickNumber > inputMsg.StartTickNumber 
                    ? tempServerTickNumber - inputMsg.StartTickNumber : 0;

                // run through all relevant inputs, and step player forward
                for (var i = (int) startIndex; i < inputMsg.Inputs.Count; ++i)
                {
                    //If we are host of player, don't simulate our player physics as we already did this
                    //But we still want to send state to other clients
                    if (!photonView.IsMine)
                    {
                        movementStateMachine.CanUpdate = true;
                        Move(inputMsg.Inputs[i].MovementDirection);

                        if (inputMsg.Inputs[i].SubmitButton == 1)
                        {
                            // OPTIONAL: adjust our player's position by 1/4th so that the input is more accurate
                            // Can cause visible lag jump (teleporting player) if players are moving fast
                            movementStateMachine.RigidBody2D.position = Vector3.Lerp(movementStateMachine.RigidBody2D.position, inputMsg.Position, 0.25f);
                            SubmitButtonApply(inputMsg.Inputs[i].MousePos, inputMsg.Inputs[i].MovementDirection, movementStateMachine.RigidBody2D);
                        }
                    }

                    ++tempServerTickNumber;
                    ++tempServerTickAccumulator;
                }

                proxyClientPlayer.position = movementStateMachine.RigidBody2D.position;
                proxyClientPlayer.rotation = movementStateMachine.RigidBody2D.rotation;
            }

            // If we're the host and master player send our state since we don't recieve messages from ourself
            if (photonView.IsMine)
            {
                ++tempServerTickNumber;
                ++tempServerTickAccumulator;

                if (tempServerTickAccumulator >= serverSnapshotRate)
                {
                    tempServerTickAccumulator = 0;
                    PlayerSyncStructs.StateMessage stateMsg;
                    stateMsg.DeliveryTime = (float) PhotonNetwork.Time;
                    stateMsg.TickNumber = tempServerTickNumber;
                    stateMsg.Position = movementStateMachine.RigidBody2D.position;
                    stateMsg.Rotation = movementStateMachine.RigidBody2D.rotation;
                    stateMsg.Velocity = movementStateMachine.RigidBody2D.velocity;
                    stateMsg.AngularVelocity = movementStateMachine.RigidBody2D.angularVelocity;
                    
                    SendClientMessage(stateMsg.DeliveryTime, (int) stateMsg.TickNumber, stateMsg.Position,
                        stateMsg.Rotation, stateMsg.Velocity, stateMsg.AngularVelocity);
                }
            }

            serverTickNumber = tempServerTickNumber;
            serverTickAccumulator = tempServerTickAccumulator;
        }
        
        /// <summary>
        /// Performs player movement 
        /// </summary>
        private void Move(Vector2 movementDirection)
        {
            movementStateMachine.MovementDirection = movementDirection;
        }
        
        /// <summary>
        /// Performs player input submit 
        /// </summary>
        private void SubmitButtonApply(Vector3 mousePos, Vector2 sitckMove, Rigidbody2D rigidbody)
        {
            // TODO: perform player input submit 
        }

        #endregion

        #region PunRrpCallbacks

        [PunRPC]
        private void SendServerMessageRPC(double deliveryTime, int startTick, Vector3 pos, IReadOnlyList<int> inputListSubmit,
            IReadOnlyList<Vector2> inputListStick, IReadOnlyList<Vector3> inputListMouse, PhotonMessageInfo info)
        {
            var inputList = new List<PlayerSyncStructs.Inputs>();
            
            for (var i = 0; i < inputListSubmit.Count; i++)
            {
                var newInput = new PlayerSyncStructs.Inputs
                {
                    SubmitButton = inputListSubmit[i], 
                    MovementDirection = inputListStick[i], 
                    MousePos = inputListMouse[i]
                };

                inputList.Add(newInput);
            }

            PlayerSyncStructs.InputMessage inputMsg;
            inputMsg.DeliveryTime = (float) deliveryTime;
            inputMsg.StartTickNumber = (uint) startTick;
            inputMsg.Position = pos;
            inputMsg.Inputs = inputList;
            serverInputMessages?.Enqueue(inputMsg);
        }

        private void SendClientMessage(float deliveryTime, int serverTick, Vector3 pos, float rot, Vector3 vel,
            float angVel)
        {
            photonView.RPC("SendClientMessageRPC", RpcTarget.Others, (double) deliveryTime, 
                serverTick, pos, rot, vel, angVel);
        }

        private void SwapPhysics(Rigidbody moving, Rigidbody target)
        {
            var position = moving.position;
            var velocity = moving.velocity;
            var angVelocity = moving.angularVelocity;
            var rotation = moving.rotation;

            moving.position = target.position;
            moving.rotation = target.rotation;
            moving.velocity = target.velocity;
            moving.angularVelocity = target.angularVelocity;

            target.position = position;
            target.rotation = rotation;
            target.velocity = velocity;
            target.angularVelocity = angVelocity;
        }
        
        #endregion


    }
}