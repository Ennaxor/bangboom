using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Bangboom.Player.Input;

namespace Bangboom.Player
{
    public class PlayerPhysicsSync : MonoBehaviourPun
    {
        #region ClassVariables

        private const int SEND_INPUT_QUEUE_FRAME_RATE = 3;
        
        // Client specific
        private float clientTimer;
        private uint clientTickNumber;
        private uint clientLastReceivedStateTick;
        private const int CClientBufferSize = 1024;
        private PlayerSyncStructs.ClientState[] clientStateBuffer; // client stores predicted moves here
        private PlayerSyncStructs.Inputs[] clientInputBuffer; // client stores predicted inputs here
        private Queue<PlayerSyncStructs.StateMessage> clientStateMessages;
        private Rigidbody2D proxyClientPlayer;
        private uint clientTickAccumulator;

        // Server specific
        public uint serverSnapshotRate;
        private uint serverTickNumber;
        private uint serverTickAccumulator;
        private Queue<PlayerSyncStructs.InputMessage> serverInputMessages;

        [SerializeField] private float smoothingSpeed = 7f;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Rigidbody2D clientRigidbody;
        [SerializeField] private InputReader inputReader;

        #endregion

        #region Initialization

        private void Awake()
        {
            clientTimer = 0.0f;
            clientTickNumber = 0;
            clientLastReceivedStateTick = 0;
            clientStateBuffer = new PlayerSyncStructs.ClientState[CClientBufferSize];
            clientInputBuffer = new PlayerSyncStructs.Inputs[CClientBufferSize];
            clientStateMessages = new Queue<PlayerSyncStructs.StateMessage>();

            serverTickNumber = 0;
            serverTickAccumulator = 0;
            serverInputMessages = new Queue<PlayerSyncStructs.InputMessage>();

            Application.targetFrameRate = 100; 
            // Limit frame rate to 100 when in online so we don't overdue our message limits since the snapshot rate is related to frame rate
            // THIS CAN AND SHOULD BE FRAME INDEPENDENT TO REMOVE THIS RESTRICTION OTHERWISE YOU WILL OVERUSE YOUR NETWORK BANDWITH
        }

        private void Start()
        {
            if (proxyClientPlayer == null)
            {
                SpawnProxyClientPlayer();
            }
            
            if (photonView.IsMine)
            {
                serverSnapshotRate = GetServerSnapshotRate();
            }
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
        
        /// <summary>
        /// Gets the server rate based on frames
        /// </summary>
        private static uint GetServerSnapshotRate()
        {
            var deltaTimeFPS = 0f;
            deltaTimeFPS += Time.deltaTime;
            deltaTimeFPS /= 2.0f;
            var fps = (int) (1.0f / deltaTimeFPS);

            // UPDATE OUR SERVER/HOST CLIENTS RATE AT WHICH WE ARE SENDING PHYSICS SNAPSHOTS BASED ON THEIR FRAME RATE OR THE PLAYER COUNT
            // We do this to limit the amount of info we send so we don't overdo network traffic. (This should be adjusted for your game and network capabilities)
            //TODO
            // serverSnapshotRate = (uint)Mathf.Max((int)(fps / 20), NetworkInputManager.instance.GetPlayerCount() * 2); EXAMPLE
            return (uint) (fps / 20);
        }

        #endregion

        private void Update()
        {
            ClientUpdate();
            ServerUpdate();
        }

        /// <summary>
        /// Performs player movement 
        /// </summary>
        /// <param name="stickMove"></param>
        private void Move(Vector2 stickMove)
        {
            clientRigidbody.velocity = stickMove * 10;
        }
        
        /// <summary>
        /// Performs player input submit 
        /// </summary>
        private void SubmitButtonApply(Vector3 mousePos, Vector2 sitckMove, Rigidbody2D rigidbody)
        {
            // TODO: perform player input submit 
        }
        
        private void OnDisable()
        {
            if (proxyClientPlayer != null)
            {
                Destroy(proxyClientPlayer);
            }
        }

        #region ClientUpdate

        /// <summary>
        /// Sends inputs if host of this player / perform corrections if on another client
        /// </summary>
        private void ClientUpdate()
        {
            var dt = Time.fixedDeltaTime;
            var tempClientTimer = clientTimer;
            var tempClientTickNumber = clientTickNumber;
            
            if (photonView.IsMine)
            {
                ManageInputs(tempClientTickNumber);
            }

            ++tempClientTickNumber;

            OtherPlayerCorrections(tempClientTickNumber, dt);

            clientTimer = tempClientTimer;
            clientTickNumber = tempClientTickNumber;
        }
        
        /// <summary>
        /// Manages host player inputs
        /// </summary>
        private void ManageInputs(uint tickNumber)
        {
            var bufferSlot = tickNumber % CClientBufferSize;

            // Sample and store inputs for this tick. Grab the inputs from InputReader
            var inputs = new PlayerSyncStructs.Inputs
            {
                MovementDirection = inputReader.MovementDirection
            };

            clientInputBuffer[bufferSlot] = inputs;

            ClientStoreCurrentStateAndStep(ref clientStateBuffer[bufferSlot], clientRigidbody, inputs);

            // If we haven't passed the client snapshot rate just queue to send (Only send our input queue every
            // SEND_INPUT_QUEUE_FRAME_RATE frame to reduce traffic) Otherwise send the queue
            //TODO UPDATE THIS CLIENT SNAPSHOT RATE BASED ON GAME AND NETWORK TRAFFIC LIMITS
            ++clientTickAccumulator;

            if (clientTickAccumulator < SEND_INPUT_QUEUE_FRAME_RATE)
            {
                return;
            }
            
            clientTickAccumulator = 0;
            
            PlayerSyncStructs.InputMessage inputMsg;
            inputMsg.DeliveryTime = (float) PhotonNetwork.Time;
            inputMsg.StartTickNumber = tickNumber - SEND_INPUT_QUEUE_FRAME_RATE;
            inputMsg.Position = clientRigidbody.position;
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
        private void OtherPlayerCorrections(uint tickNumber, float dt)
        {
            if (!ClientHasStateMessage())
            {
                return;
            }
            
            var recievedStateMsg = clientStateMessages.Dequeue();
            
            // Make sure if there are any newer state messages available, we use those instead
            while (ClientHasStateMessage()) 
            {
                recievedStateMsg = clientStateMessages.Dequeue();
            }

            clientLastReceivedStateTick = recievedStateMsg.TickNumber;

            var bufferSlot = recievedStateMsg.TickNumber % CClientBufferSize;
            var positionError = recievedStateMsg.Position - clientStateBuffer[bufferSlot].Position;
            // TODO: proper rotation error calculation
            var rotationError = 0;

            if (!(positionError.sqrMagnitude > 0.2f) && !(rotationError > 0.1f))
            {
                return;
            }
            
            if (photonView.IsMine)
            {
                Debug.Log("Correcting for error at tick " + recievedStateMsg.TickNumber + 
                          " (rewinding " + (tickNumber - recievedStateMsg.TickNumber) + " ticks)");
            }
                    
            // If the packet is valid update our network proxy
            if (!float.IsNaN(recievedStateMsg.Position.x))
            {
                proxyClientPlayer.position = Vector3.Lerp(proxyClientPlayer.position,
                    recievedStateMsg.Position, 0.5f);
                proxyClientPlayer.rotation = recievedStateMsg.Rotation;
                proxyClientPlayer.velocity = recievedStateMsg.Velocity;
                proxyClientPlayer.angularVelocity = recievedStateMsg.AngularVelocity;
            }

            // If the packet is valid update skew our visible players velociy to anticipate corrections
            if (!float.IsNaN(recievedStateMsg.Velocity.x))
            {
                clientRigidbody.velocity = Vector3.Slerp(clientRigidbody.velocity, recievedStateMsg.Velocity, 0.3f);
            }

            // If we're past threshold, just snap or If we're going fast be less likely to snap
            if (float.IsNaN(recievedStateMsg.Position.x))
            {
                return;
            }
            
            if ((recievedStateMsg.Position - clientRigidbody.position).sqrMagnitude >=
                Mathf.Clamp(recievedStateMsg.Velocity.magnitude, 1f, 4.5f)
                || Mathf.Abs(recievedStateMsg.Velocity.magnitude - clientRigidbody.velocity.magnitude)
                >= 6f)
            {
                clientRigidbody.position = proxyClientPlayer.position;
                clientRigidbody.rotation = proxyClientPlayer.rotation;
                clientRigidbody.velocity = proxyClientPlayer.velocity;
                clientRigidbody.angularVelocity = proxyClientPlayer.angularVelocity;
            }
            
            if (photonView.IsMine)
            {
                return;
            }
            
            if ((proxyClientPlayer.position - clientRigidbody.position).sqrMagnitude >= .1f)
            {
                clientRigidbody.position = Vector3.Lerp(clientRigidbody.position, proxyClientPlayer.position,
                    dt * smoothingSpeed);
            }
        }
        
        /// <summary>
        /// Stores state for this tick and uses the current state and inputs to step simulation
        /// </summary>
        private void ClientStoreCurrentStateAndStep(ref PlayerSyncStructs.ClientState currentState,
            Rigidbody2D rigidbody, PlayerSyncStructs.Inputs inputs)
        {
            currentState.Position = rigidbody.position;
            currentState.Rotation = rigidbody.rotation;

            Move(inputs.MovementDirection);

            if (inputs.SubmitButton == 1)
            {
                SubmitButtonApply(inputs.MousePos, inputs.MovementDirection, rigidbody);
            }
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

        #region ServerUpdate
        
        /// <summary>
        /// Sends updated if host of this player / perform inputs if on another player
        /// </summary>
        private void ServerUpdate()
        {
            var tempServerTickNumber = serverTickNumber;
            var tempServerTickAccumulator = serverTickAccumulator;

            PlayerSyncStructs.StateMessage stateMsg;

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
                        Move(inputMsg.Inputs[i].MovementDirection);

                        if (inputMsg.Inputs[i].SubmitButton == 1)
                        {
                            // OPTIONAL: adjust our player's position by 1/4th so that the input is more accurate
                            // Can cause visible lag jump (teleporting player) if players are moving fast
                            clientRigidbody.position =
                                Vector3.Lerp(clientRigidbody.position, inputMsg.Position, 0.25f);
                            SubmitButtonApply(inputMsg.Inputs[i].MousePos, inputMsg.Inputs[i].MovementDirection,
                                clientRigidbody);
                        }
                    }

                    ++tempServerTickNumber;
                    ++tempServerTickAccumulator;
                }

                proxyClientPlayer.position = clientRigidbody.position;
                proxyClientPlayer.rotation = clientRigidbody.rotation;
            }

            // If we're the host and master player send our state since we don't recieve messages from ourself
            if (photonView.IsMine)
            {
                ++tempServerTickNumber;
                ++tempServerTickAccumulator;

                if (tempServerTickAccumulator >= serverSnapshotRate)
                {
                    tempServerTickAccumulator = 0;
                    stateMsg.DeliveryTime = (float) PhotonNetwork.Time;
                    stateMsg.TickNumber = tempServerTickNumber;
                    stateMsg.Position = clientRigidbody.position;
                    stateMsg.Rotation = clientRigidbody.rotation;
                    stateMsg.Velocity = clientRigidbody.velocity;
                    stateMsg.AngularVelocity = clientRigidbody.angularVelocity;
                    
                    SendClientMessage(stateMsg.DeliveryTime, (int) stateMsg.TickNumber, stateMsg.Position,
                        stateMsg.Rotation, stateMsg.Velocity, stateMsg.AngularVelocity);
                }
            }

            serverTickNumber = tempServerTickNumber;
            serverTickAccumulator = tempServerTickAccumulator;
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
            
            clientRigidbody.velocity = velocity;
            clientRigidbody.position = pos + velocity * Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            proxyClientPlayer.position = clientRigidbody.position;
            proxyClientPlayer.velocity = clientRigidbody.velocity;
        }
        
        #endregion


    }
}