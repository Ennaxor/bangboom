using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Bangboom.Player.Server;
using Bangboom.StateMachine;

namespace Bangboom.Player.Client
{
    public class PlayerClient : MonoBehaviourPun
    {
        #region ClassVariables

        [SerializeField] private float smoothingSpeed = 7f;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private MovementStateMachine movementStateMachine;
        [SerializeField] private PlayerServer playerServer;

        private const int SEND_INPUT_QUEUE_FRAME_RATE = 3;
        
        // Client specific
        private float clientTimer;
        private uint clientTickNumber;
        private const int CClientBufferSize = 1024;
        private PlayerSyncStructs.ClientState[] clientStateBuffer; 
        private PlayerSyncStructs.Inputs[] clientInputBuffer; 
        private Queue<PlayerSyncStructs.StateMessage> clientStateMessages;
        private Rigidbody2D proxyClientPlayer;
        private uint clientTickAccumulator;

        #endregion

        #region Initialization

        private void Awake()
        {
            clientTimer = 0.0f;
            clientTickNumber = 0;
            clientStateBuffer = new PlayerSyncStructs.ClientState[CClientBufferSize];
            clientInputBuffer = new PlayerSyncStructs.Inputs[CClientBufferSize];
            clientStateMessages = new Queue<PlayerSyncStructs.StateMessage>();
            Application.targetFrameRate = 100; 
        }

        private void Start()
        {
            if (proxyClientPlayer == null)
            {
                SpawnProxyClientPlayer();
            }
            
            movementStateMachine.CanUpdate = true;
            playerServer.SetProxyClientPlayer(proxyClientPlayer);
        }
        
        /// <summary>
        /// Spawns a Proxy Player (represents the position of the networked player)
        /// </summary>
        private void SpawnProxyClientPlayer()
        {
            var temp = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            proxyClientPlayer = temp.GetComponent<Rigidbody2D>();
            proxyClientPlayer.position = transform.position;
        }
        
        private void OnDisable()
        {
            if (proxyClientPlayer != null)
            {
                Destroy(proxyClientPlayer);
            }
        }

        #endregion

        private void Update()
        {
            ClientUpdate();
        }

        #region ClientUpdate

        /// <summary>
        /// Sends inputs if host of this player / perform corrections if on another client
        /// </summary>
        private void ClientUpdate()
        {
            var tempClientTimer = clientTimer;
            var tempClientTickNumber = clientTickNumber;
            
            if (photonView.IsMine)
            {
                ManageInputs(tempClientTickNumber);
            }

            ++tempClientTickNumber;

            OtherPlayerCorrections(Time.fixedDeltaTime);

            clientTimer = tempClientTimer;
            clientTickNumber = tempClientTickNumber;
        }
        
        /// <summary>
        /// Manages host player inputs
        /// </summary>
        private void ManageInputs(uint tickNumber)
        {
            var bufferSlot = tickNumber % CClientBufferSize;
            var inputs = new PlayerSyncStructs.Inputs
            {
                MovementDirection = movementStateMachine.InputReader.MovementDirection
            };

            clientInputBuffer[bufferSlot] = inputs;
            ClientStoreCurrentStateAndStep(ref clientStateBuffer[bufferSlot], movementStateMachine.RigidBody2D, inputs);
            ++clientTickAccumulator;

            if (clientTickAccumulator < SEND_INPUT_QUEUE_FRAME_RATE)
            {
                return;
            }
            
            clientTickAccumulator = 0;
            
            PlayerSyncStructs.InputMessage inputMsg;
            inputMsg.DeliveryTime = (float) PhotonNetwork.Time;
            inputMsg.StartTickNumber = tickNumber - SEND_INPUT_QUEUE_FRAME_RATE;
            inputMsg.Position = movementStateMachine.RigidBody2D.position;
            inputMsg.Inputs = new List<PlayerSyncStructs.Inputs>();

            var startTick = inputMsg.StartTickNumber;
            for (var tick = startTick; tick <= tickNumber; ++tick)
            {
                inputMsg.Inputs.Add(clientInputBuffer[tick % CClientBufferSize]);
            }

            SendServerMessage(inputMsg);
        }
         
        /// <summary>
        /// Manages other player corrections
        /// </summary>
        private void OtherPlayerCorrections(float dt)
        {
            if (!ClientHasStateMessage())
            {
                return;
            }
            
            var receivedStateMsg = clientStateMessages.Dequeue();
            while (ClientHasStateMessage()) 
            {
                receivedStateMsg = clientStateMessages.Dequeue();
            }

            var bufferSlot = receivedStateMsg.TickNumber % CClientBufferSize;
            var positionError = receivedStateMsg.Position - clientStateBuffer[bufferSlot].Position;
            var rotationError = 0;

            if (!(positionError.sqrMagnitude > 0.2f) && !(rotationError > 0.1f))
            {
                return;
            }
                    
            // If the packet is valid update our network proxy
            if (!float.IsNaN(receivedStateMsg.Position.x))
            {
                Debug.LogWarning("------------------------- 1");
                proxyClientPlayer.position = Vector3.Lerp(proxyClientPlayer.position,
                    receivedStateMsg.Position, 0.5f);
                proxyClientPlayer.rotation = receivedStateMsg.Rotation;
                proxyClientPlayer.velocity = receivedStateMsg.Velocity;
                proxyClientPlayer.angularVelocity = receivedStateMsg.AngularVelocity;
            }

            // If the packet is valid update skew our visible players velociy to anticipate corrections
            if (!float.IsNaN(receivedStateMsg.Velocity.x))
            {
                Debug.LogWarning("------------------------- 2");
                movementStateMachine.RigidBody2D.velocity = Vector3.Slerp(movementStateMachine.RigidBody2D.velocity, receivedStateMsg.Velocity, 0.3f);
            }

            // If we're past threshold, just snap or If we're going fast be less likely to snap
            if (float.IsNaN(receivedStateMsg.Position.x))
            {
                return;
            }
            
            if ((receivedStateMsg.Position - movementStateMachine.RigidBody2D.position).sqrMagnitude >=
                Mathf.Clamp(receivedStateMsg.Velocity.magnitude, 1f, 4.5f)
                || Mathf.Abs(receivedStateMsg.Velocity.magnitude - movementStateMachine.RigidBody2D.velocity.magnitude)
                >= 6f)
            {
                movementStateMachine.RigidBody2D.position = proxyClientPlayer.position;
                movementStateMachine.RigidBody2D.rotation = proxyClientPlayer.rotation;
                movementStateMachine.RigidBody2D.velocity = proxyClientPlayer.velocity;
                movementStateMachine.RigidBody2D.angularVelocity = proxyClientPlayer.angularVelocity;
            }
            
            if (photonView.IsMine)
            {
                return;
            }
            
            if ((proxyClientPlayer.position - movementStateMachine.RigidBody2D.position).sqrMagnitude >= .1f)
            {
                Debug.LogWarning("------------------------- 3");
                movementStateMachine.RigidBody2D.position = Vector3.Lerp(movementStateMachine.RigidBody2D.position, proxyClientPlayer.position,
                    dt * smoothingSpeed);
            }
        }
        
        /// <summary>
        /// Stores state for this tick and uses the current state and inputs to step simulation
        /// </summary>
        private void ClientStoreCurrentStateAndStep(ref PlayerSyncStructs.ClientState currentState,
            Rigidbody2D rigidBody, PlayerSyncStructs.Inputs inputs)
        {
            currentState.Position = rigidBody.position;
            currentState.Rotation = rigidBody.rotation;
            
            //Move(inputs.MovementDirection);

            if (inputs.SubmitButton == 1)
            {
                SubmitButtonApply(inputs.MousePos, inputs.MovementDirection, rigidBody);
            }
        }

        /// <summary>
        /// Performs player movement 
        /// </summary>
      /*  private void Move(Vector2 movementDirection)
        {
            clientRigidbody.velocity = movementDirection * 10;
        }*/
        
        /// <summary>
        /// Performs player input submit 
        /// </summary>
        private void SubmitButtonApply(Vector3 mousePos, Vector2 move, Rigidbody2D rigidBody)
        {
            // TODO: perform player input submit 
        }
        
        /// <summary>
        /// Sends inputs to server
        /// </summary>
        private void SendServerMessage(PlayerSyncStructs.InputMessage clientInputMsg)
        {
            var deliveryTime = (double) clientInputMsg.DeliveryTime;
            var startTick = (int) clientInputMsg.StartTickNumber;

            //We will save all inputs in long lists that we will index for diff messages using inputsLength
            var inputListSubmit = new List<int>();
            var inputListStick = new List<Vector2>();
            var inputListMouse = new List<Vector3>();

            for (var i = 0; i < clientInputMsg.Inputs.Count; i++)
            {
                //Save the length of the inputlist to expect when recieving
                //This lets us find at which point in the lists a new message begins
                var inputList = clientInputMsg.Inputs;

                foreach (var input in inputList)
                {
                    inputListSubmit.Add(input.SubmitButton);
                    inputListStick.Add(input.MovementDirection);
                    inputListMouse.Add(input.MousePos);
                }
            }

            photonView.RPC("SendServerMessageRPC", RpcTarget.Others, deliveryTime, startTick,
                clientInputMsg.Position, inputListSubmit.ToArray(), inputListStick.ToArray(), inputListMouse.ToArray());
        }
        
        private bool ClientHasStateMessage()
        {
            return clientStateMessages.Count > 0 && (float) PhotonNetwork.Time >= clientStateMessages.Peek().DeliveryTime;
        }

        #endregion

        #region PunRrpCallbacks

        private void SendClientMessage(float deliveryTime, int serverTick, Vector3 pos, float rot, Vector3 vel,
            float angVel)
        {
            photonView.RPC("SendClientMessageRPC", RpcTarget.Others, (double) deliveryTime, 
                serverTick, pos, rot, vel, angVel);
        }


        [PunRPC]
        private void SendClientMessageRPC(double deliveryTime, int serverTick, Vector3 pos, float rot, Vector3 vel,
            float angVel, PhotonMessageInfo info)
        {
            PlayerSyncStructs.StateMessage stateMsg;
            stateMsg.DeliveryTime = (float) deliveryTime;
            stateMsg.TickNumber = (uint) serverTick;
            stateMsg.Position = pos;
            stateMsg.Rotation = rot;
            stateMsg.Velocity = vel;
            stateMsg.AngularVelocity = angVel;
            clientStateMessages?.Enqueue(stateMsg);
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

        [PunRPC]
        private void SendCollisionRPC(Vector3 pos, Vector3 velocity, PhotonMessageInfo info)
        {
            if (photonView.IsMine)
            {
                return;
            }
            
            movementStateMachine.RigidBody2D.velocity = velocity;
            movementStateMachine.RigidBody2D.position = pos + velocity * Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            proxyClientPlayer.position = movementStateMachine.RigidBody2D.position;
            proxyClientPlayer.velocity = movementStateMachine.RigidBody2D.velocity;
        }
        
        #endregion

    }
}