using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace KiwiManager
{
    class TrafficPriority
    {
        public static bool leftHandDrive;

        public static Dictionary<uint, PrioritySegment> prioritySegments = new Dictionary<uint, PrioritySegment>();

        public static Dictionary<ushort, PriorityCar> vehicleList = new Dictionary<ushort, PriorityCar>();

        public static bool[] changedTarget = new bool[NetManager.MAX_SEGMENT_COUNT];

        private static uint getKey(ushort nodeID, ushort segmentID)
        {
            if (nodeID != 0 && nodeID < NetManager.MAX_NODE_COUNT &&
                segmentID != 0 && segmentID < NetManager.MAX_SEGMENT_COUNT)
            {
                return (uint) ((nodeID << 16) | segmentID);
            }
            else
            {
                throw new IndexOutOfRangeException("node/segment ID " + nodeID + "/" + segmentID);
            }
        }
        public static void addPrioritySegment(ushort nodeID, ushort segmentID, PrioritySegment.PriorityType type)
        {
            try
            {
                prioritySegments.Add(getKey(nodeID, segmentID), new PrioritySegment(nodeID, segmentID, type));
            }
            catch (IndexOutOfRangeException ioore)
            {
            }
        }

        public static void removePrioritySegment(ushort nodeID, ushort segmentID)
        {
            try
            {
                prioritySegments.Remove(getKey(nodeID, segmentID));
            }
            catch (IndexOutOfRangeException ioore)
            {
            }
        }

        public static bool isPrioritySegment(ushort nodeID, ushort segmentID)
        {
            try
            {
                return prioritySegments.ContainsKey(getKey(nodeID, segmentID));
            }
            catch (IndexOutOfRangeException ioore)
            {
                return false;
            }
        }

        public static PrioritySegment getPrioritySegment(ushort nodeID, ushort segmentID)
        {
            try
            {
                return prioritySegments[getKey(nodeID, segmentID)];
            }
            catch (IndexOutOfRangeException ioore)
            {
                return null;
            }
            catch (KeyNotFoundException knfe)
            {
                return null;
            }
        }

        public static bool incomingVehicles(ushort targetCar, ushort nodeID)
        {
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            uint frame = currentFrameIndex >> 4;
            var node = TrafficLightTool.GetNetNode(nodeID);

            var fromPrioritySegment = getPrioritySegment(nodeID, vehicleList[targetCar].fromSegment);

            List<ushort> removeCarList = new List<ushort>();

            var numCars = 0;

            // get all cars
            for (int s = 0; s < 8; s++)
            {
                var segment = node.GetSegment(s);

                if (segment != 0 && segment != vehicleList[targetCar].fromSegment)
                {
                    if (isPrioritySegment(nodeID, segment))
                    {
                        var prioritySegment = getPrioritySegment(nodeID, segment);

                        // select outdated cars
                        foreach (var car in prioritySegment.cars)
                        {
                            var frameReduce = vehicleList[car].lastSpeed < 70 ? 4u : 2u;

                            if (vehicleList[car].lastFrame < frame - frameReduce)
                            {
                                removeCarList.Add(car);
                            }
                        }

                        // remove outdated cars
                        foreach (var rcar in removeCarList)
                        {
                            vehicleList[rcar].resetCar();
                            prioritySegment.RemoveCar(rcar);
                        }

                        removeCarList.Clear();

                        if ((node.m_flags & NetNode.Flags.TrafficLights) == NetNode.Flags.None)
                        {
                            if (fromPrioritySegment.ComparablePriority <= prioritySegment.ComparablePriority)
                            {
                                numCars += prioritySegment.cars.Count;
                                if (fromPrioritySegment.ComparablePriority < prioritySegment.ComparablePriority)
                                {
                                    foreach (ushort car in prioritySegment.cars)
                                    {
                                        if (checkSameRoadIncomingCar(targetCar, car, nodeID, true))
                                        {
                                            numCars--;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (ushort car in prioritySegment.cars)
                                    {
                                        if (vehicleList[car].lastSpeed <= 0.1f || checkSameRoadIncomingCar(targetCar, car, nodeID))
                                        {
                                            numCars--;
                                        }
                                    }
                                }

                            }
                            /*
                            if (fromPrioritySegment.type == PrioritySegment.PriorityType.Main)
                            {
                                if (prioritySegment.type == PrioritySegment.PriorityType.Main)
                                {
                                    numCars += prioritySegment.cars.Count;

                                    foreach (ushort car in prioritySegment.cars)
                                    {
                                        if (vehicleList[car].lastSpeed <= 0.1f || checkSameRoadIncomingCar(targetCar, car, nodeID))
                                        {
                                            numCars--;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                numCars += prioritySegment.cars.Count;

                                foreach (ushort car in prioritySegment.cars)
                                {
                                    if (prioritySegment.type == PrioritySegment.PriorityType.Main)
                                    {
                                        if (vehicleList[car].stopped || checkSameRoadIncomingCar(targetCar, car, nodeID))
                                        {
                                            numCars--;
                                        }
                                    }
                                    else
                                    {
                                        if (vehicleList[car].lastSpeed <= 0.1f || checkSameRoadIncomingCar(targetCar, car, nodeID))
                                        {
                                            numCars--;
                                        }
                                    }
                                }
                            }
                            */
                        }
                        else
                        {
                            if (TrafficLightsManual.IsSegmentLight(nodeID, segment))
                            {
                                var segmentLight = TrafficLightsManual.GetSegmentLight(nodeID, segment);

                                if (segmentLight.GetLightMain() == RoadBaseAI.TrafficLightState.Green)
                                {
                                    numCars += prioritySegment.cars.Count;

                                    foreach (ushort car in prioritySegment.cars)
                                    {
                                        if (vehicleList[car].lastSpeed <= 1f || checkSameRoadIncomingCar(targetCar, car, nodeID))
                                        {
                                            numCars--;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (numCars > 0);
        }

        public static bool checkSameRoadIncomingCar(ushort targetCarID, ushort incomingCarID, ushort nodeID, bool giveway = false)
        {
            if (leftHandDrive)
            {
                return _checkSameRoadIncomingCarLeftHandDrive(targetCarID, incomingCarID, nodeID, giveway);
            }
            else
            {
                return _checkSameRoadIncomingCarRightHandDrive(targetCarID, incomingCarID, nodeID);
            }
        }

        protected static bool _checkSameRoadIncomingCarLeftHandDrive(ushort targetCarID, ushort incomingCarID,
            ushort nodeID, bool giveway = false)
        {
            var targetCar = vehicleList[targetCarID];
            var incomingCar = vehicleList[incomingCarID];
            if (targetCar.fromSegment == incomingCar.fromSegment) return true;
            if (!giveway && isForwardSegment(targetCar.fromSegment, targetCar.toSegment, nodeID) && !isForwardSegment(incomingCar.fromSegment, incomingCar.toSegment, nodeID)) return true;
            sbyte aheadAngle = (sbyte) (getAngle(nodeID, targetCar.fromSegment) + 128);
            sbyte targetToAngle = (sbyte) -(getAngle(nodeID, targetCar.toSegment) - aheadAngle);
            sbyte incomingFromAngle = (sbyte) -(getAngle(nodeID, incomingCar.fromSegment) - aheadAngle);
            sbyte incomingToAngle = (sbyte) -(getAngle(nodeID, incomingCar.toSegment) - aheadAngle);
            if (incomingFromAngle <= targetToAngle)
            {
                if (incomingToAngle < targetToAngle || (incomingToAngle == targetToAngle &&
                    laneOrderCorrect(targetCar.toSegment, targetCar.toLaneID, incomingCar.toLaneID)))
                {
                    return true; // no conflict
                }
            }
            else
            {
                if (incomingToAngle > targetToAngle || (incomingToAngle == targetToAngle &&
                    laneOrderCorrect(targetCar.toSegment, incomingCar.toLaneID, targetCar.toLaneID)))
                {
                    return true; // no conflict
                }
            }
            if (giveway) return false;
            if (!isForwardSegment(targetCar.fromSegment, targetCar.toSegment, nodeID) && isForwardSegment(incomingCar.fromSegment, incomingCar.toSegment, nodeID)) return false;

            targetToAngle = (sbyte) -targetToAngle;
            incomingFromAngle = (sbyte) -incomingFromAngle;
            return (incomingFromAngle < targetToAngle);
        }

        protected static sbyte getAngle(ushort nodeID, int segmentID)
        {
            Vector3 v = GetSegmentDir(segmentID, nodeID);
            return (sbyte) Math.Round(Math.Atan2(v.x, v.z) * 128 / Math.PI);
        }
        protected static bool _checkSameRoadIncomingCarRightHandDrive(ushort targetCarID, ushort incomingCarID,
            ushort nodeID)
        {
            var targetCar = vehicleList[targetCarID];
            var incomingCar = vehicleList[incomingCarID];
            if (targetCar.fromSegment == incomingCar.fromSegment) return true;
            sbyte aheadAngle = (sbyte) (getAngle(nodeID, targetCar.fromSegment) + 128);
            sbyte targetToAngle = (sbyte) (getAngle(nodeID, targetCar.toSegment) - aheadAngle);
            sbyte incomingFromAngle = (sbyte) (getAngle(nodeID, incomingCar.fromSegment) - aheadAngle);
            sbyte incomingToAngle = (sbyte) (getAngle(nodeID, incomingCar.toSegment) - aheadAngle);

            if (incomingFromAngle <= targetToAngle) return true;
            if (incomingToAngle == targetToAngle) return laneOrderCorrect(targetCar.toSegment, targetCar.toLaneID, incomingCar.toLaneID);
            return (incomingToAngle >= targetToAngle);
        }


        public static bool laneOrderCorrect(int segmentid, uint leftLane, uint rightLane)
        {
            NetManager instance = Singleton<NetManager>.instance;

            var segment = instance.m_segments.m_buffer[segmentid];
            var info = segment.Info;

            uint num2 = segment.m_lanes;
            int num3 = 0;

            var oneWaySegment = true;

            while (num3 < info.m_lanes.Length && num2 != 0u)
            {
                if (info.m_lanes[num3].m_laneType != NetInfo.LaneType.Pedestrian &&
                    (info.m_lanes[num3].m_direction == NetInfo.Direction.Backward))
                {
                    oneWaySegment = false;
                }

                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                num3++;
            }

            num3 = 0;
            float leftLanePosition = 0f;
            float rightLanePosition = 0f;

            while (num3 < info.m_lanes.Length && num2 != 0u)
            {
                if (num2 == leftLane)
                {
                    leftLanePosition = info.m_lanes[num3].m_position;
                }

                if (num2 == rightLane)
                {
                    rightLanePosition = info.m_lanes[num3].m_position;
                }

                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                num3++;
            }

            if (oneWaySegment)
            {
                if (leftLanePosition < rightLanePosition)
                {
                    return true;
                }
            }
            else
            {
                if (leftLanePosition > rightLanePosition)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool isRightSegment(int fromSegment, int toSegment, ushort nodeid)
        {
            return isLeftSegment(toSegment, fromSegment, nodeid);
        }

        public static bool isLeftSegment(int fromSegment, int toSegment, ushort nodeid)
        {
            Vector3 fromDir = GetSegmentDir(fromSegment, nodeid);
            fromDir.y = 0;
            fromDir.Normalize();
            Vector3 toDir = GetSegmentDir(toSegment, nodeid);
            toDir.y = 0;
            toDir.Normalize();
            return Vector3.Cross(fromDir, toDir).y >= 0.5;
        }

        public static bool isForwardSegment(int fromSegment, int toSegment, ushort nodeid)
        {
            Vector3 fromDir = GetSegmentDir(fromSegment, nodeid);
            fromDir.y = 0;
            fromDir.Normalize();
            Vector3 toDir = GetSegmentDir(toSegment, nodeid);
            toDir.y = 0;
            toDir.Normalize();
            return Math.Abs(Vector3.Cross(fromDir, toDir).y) < 0.5;
        }

        public static NetLane.Flags GetTurnDirection(int fromSegment, int toSegment, ushort nodeid)
        {
            Vector3 fromDir = GetSegmentDir(fromSegment, nodeid);
            fromDir.y = 0;
            fromDir.Normalize();
            Vector3 toDir = GetSegmentDir(toSegment, nodeid);
            toDir.y = 0;
            toDir.Normalize();
            float cross = Vector3.Cross(fromDir, toDir).y;
            if (Mathf.Abs(cross) < 0.5) return NetLane.Flags.Forward;
            return cross < 0 ? NetLane.Flags.Right : NetLane.Flags.Left;
        }

        public static bool HasLeftSegment(int segmentID, ushort nodeID, bool debug = false)
        {
            var node = TrafficLightTool.GetNetNode(nodeID);

            for (int s = 0; s < 8; s++)
            {
                var segment = node.GetSegment(s);

                if (segment != 0 && segment != segmentID)
                {
                    if (isLeftSegment(segmentID, segment, nodeID))
                    {
                        if (debug)
                        {
                            Debug.Log("LEFT: " + segment + " " + GetSegmentDir(segment, nodeID));
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasRightSegment(int segmentID, ushort nodeID, bool debug = false)
        {
            var node = TrafficLightTool.GetNetNode(nodeID);

            for (int s = 0; s < 8; s++)
            {
                var segment = node.GetSegment(s);

                if (segment != 0 && segment != segmentID)
                {
                    if (isRightSegment(segmentID, segment, nodeID))
                    {
                        if (debug)
                        {
                            Debug.Log("RIGHT: " + segment + " " + GetSegmentDir(segment, nodeID));
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasForwardSegment(int segmentID, ushort nodeID, bool debug = false)
        {
            var node = TrafficLightTool.GetNetNode(nodeID);

            for (int s = 0; s < 8; s++)
            {
                var segment = node.GetSegment(s);

                if (segment != 0 && segment != segmentID)
                {
                    if (!isRightSegment(segmentID, segment, nodeID) && !isLeftSegment(segmentID, segment, nodeID))
                    {
                        if (debug)
                        {
                            Debug.Log("FORWARD: " + segment + " " + GetSegmentDir(segment, nodeID));
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool hasLeftLane(ushort nodeID, int segmentID)
        {
            var instance = Singleton<NetManager>.instance;
            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
            NetInfo.Direction dir = NetInfo.Direction.Forward;
            if (segment.m_startNode == nodeID)
                dir = NetInfo.Direction.Backward;
            var dir2 = ((segment.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None) ? dir : NetInfo.InvertDirection(dir);
            var dir3 = TrafficPriority.leftHandDrive ? NetInfo.InvertDirection(dir2) : dir2;

            var info = segment.Info;

            var maxValue = 0f;

            var num2 = segment.m_lanes;
            var num3 = 0;

            while (num3 < info.m_lanes.Length && num2 != 0u)
            {
                var flags = (NetLane.Flags)Singleton<NetManager>.instance.m_lanes.m_buffer[(int)num2].m_flags;

                if (info.m_lanes[num3].m_direction == dir3 && (flags & NetLane.Flags.Left) == NetLane.Flags.Left)
                {
                    return true;
                }

                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                num3++;
            }

            return false;
        }

        public static bool hasForwardLane(ushort nodeID, int segmentID)
        {
            var instance = Singleton<NetManager>.instance;
            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
            NetInfo.Direction dir = NetInfo.Direction.Forward;
            if (segment.m_startNode == nodeID)
                dir = NetInfo.Direction.Backward;
            var dir2 = ((segment.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None) ? dir : NetInfo.InvertDirection(dir);
            var dir3 = TrafficPriority.leftHandDrive ? NetInfo.InvertDirection(dir2) : dir2;

            var info = segment.Info;

            var maxValue = 0f;

            var num2 = segment.m_lanes;
            var num3 = 0;

            while (num3 < info.m_lanes.Length && num2 != 0u)
            {
                var flags = (NetLane.Flags)Singleton<NetManager>.instance.m_lanes.m_buffer[(int)num2].m_flags;

                if (info.m_lanes[num3].m_direction == dir3 && (flags & NetLane.Flags.Forward) == NetLane.Flags.Forward)
                {
                    return true;
                }

                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                num3++;
            }

            return false;
        }

        public static bool hasRightLane(ushort nodeID, int segmentID)
        {
            var instance = Singleton<NetManager>.instance;
            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
            NetInfo.Direction dir = NetInfo.Direction.Forward;
            if (segment.m_startNode == nodeID)
                dir = NetInfo.Direction.Backward;
            var dir2 = ((segment.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None) ? dir : NetInfo.InvertDirection(dir);
            var dir3 = TrafficPriority.leftHandDrive ? NetInfo.InvertDirection(dir2) : dir2;

            var info = segment.Info;

            var maxValue = 0f;

            var num2 = segment.m_lanes;
            var num3 = 0;

            while (num3 < info.m_lanes.Length && num2 != 0u)
            {
                var flags = (NetLane.Flags)Singleton<NetManager>.instance.m_lanes.m_buffer[(int)num2].m_flags;

                if (info.m_lanes[num3].m_direction == dir3 && (flags & NetLane.Flags.Right) == NetLane.Flags.Right)
                {
                    return true;
                }

                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                num3++;
            }

            return false;
        }   

        public static Vector3 GetSegmentDir(int segment, ushort nodeid)
        {
            NetManager instance = Singleton<NetManager>.instance;

            Vector3 dir;

            if (instance.m_segments.m_buffer[(int)segment].m_startNode == nodeid)
            {
                dir = instance.m_segments.m_buffer[(int)segment].m_startDirection;
            }
            else
            {
                dir = instance.m_segments.m_buffer[(int)segment].m_endDirection;
            }

            return dir;
        }
    }

    class PriorityCar
    {
        public enum CarState
        {
            None,
            Enter,
            Transit,
            Stop,
            Leave
        }

        public CarState carState = CarState.None;

        public int waitTime = 0;

        public ushort toNode;
        public ushort fromSegment;
        public ushort toSegment;
        public ushort toLaneID;
        public ushort fromLaneID;
        public ushort fromLaneFlags;
        public float lastSpeed;
        public float yieldSpeedReduce;
        public bool stopped = false;

        public uint lastFrame;

        public void resetCar()
        {
            toNode = 0;
            fromSegment = 0;
            toSegment = 0;
            toLaneID = 0;
            fromLaneID = 0;
            fromLaneFlags = 0;
            stopped = false;

            waitTime = 0;
            carState = CarState.None;
        }
    }

    public class PrioritySegment
    {
        public enum PriorityType
        {
            None = 0,
            Main = 1,
            Stop = 2,
            Yield = 3
        }

        public ushort nodeid;
        public int segmentid;


        public PriorityType type = PriorityType.Main;

        public int ComparablePriority
        {
            get
            {
                return 2 ^ (int) type;
            }
        }

        public List<ushort> cars = new List<ushort>(); 

        public int[] carsOnLanes = new int[24]; 

        public PrioritySegment(ushort nodeid, int segmentid, PriorityType type)
        {
            this.nodeid = nodeid;
            this.segmentid = segmentid;
            this.type = type;
        }

        public void AddCar(ushort vehicleID)
        {
            if (!cars.Contains(vehicleID))
            {
                cars.Add(vehicleID);
                _numCarsOnLane();
            }
        }

        public void RemoveCar(ushort vehicleID)
        {
            if (cars.Contains(vehicleID))
            {
                cars.Remove(vehicleID);
                _numCarsOnLane();
            }
        }

        public bool HasCar(ushort vehicleID)
        {
            return cars.Contains(vehicleID);
        }

        public int getCarsOnLane(int lane)
        {
            return carsOnLanes[lane];
        }

        private void _numCarsOnLane()
        {
            NetManager instance = Singleton<NetManager>.instance;

            var segment = instance.m_segments.m_buffer[this.segmentid];
            var info = segment.Info;

            uint num2 = segment.m_lanes;
            int num3 = 0;

            carsOnLanes = new int[16];

            while (num3 < info.m_lanes.Length && num2 != 0u)
            {
                if (info.m_lanes[num3].m_laneType != NetInfo.LaneType.Pedestrian) {
                    for (var i = 0; i < cars.Count; i++)
                    {
                        if (TrafficPriority.vehicleList[cars[i]].fromLaneID == num2)
                        {
                            carsOnLanes[num3]++;
                        }
                    }
                }

                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                num3++;
            }
        }
    }
}
