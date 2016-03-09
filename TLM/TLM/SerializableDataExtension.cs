using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using UnityEngine;

namespace KiwiManager
{
    public class SerializableDataExtension : ISerializableDataExtension
    {
        public static string dataID = "TrafficManager_v0.9";
        public static UInt32 uniqueID;

        public static ISerializableData SerializableData;

        private static Timer _timer;

        private static bool loaded = false;

        public void OnCreated(ISerializableData serializableData)
        {
            uniqueID = 0u;
            SerializableData = serializableData;
            loaded = false;
        }

        public void OnReleased()
        {
            loaded = false;
        }

        public static void GenerateUniqueID()
        {
            uniqueID = (uint)UnityEngine.Random.Range(1000000f, 2000000f);

            while (File.Exists(Path.Combine(Application.dataPath, "kiwiManagerSave_" + uniqueID + ".xml")))
            {
                uniqueID = (uint)UnityEngine.Random.Range(1000000f, 2000000f);
            }
        }

        public void OnLoadData()
        {
            byte[] data = SerializableData.LoadData(dataID);

            if (data == null)
            {
                Log.Message("No serialized ID");
                GenerateUniqueID();
            }
            else
            {
                _timer = new System.Timers.Timer(2000);
                // Hook up the Elapsed event for the timer. 
                _timer.Elapsed += OnLoadDataTimed;
                _timer.Enabled = true;
            }
        }

        private static void OnLoadDataTimed(System.Object source, ElapsedEventArgs e)
        {
            byte[] data = SerializableData.LoadData(dataID);

            uniqueID = 0u;

            for (var i = 0; i < data.Length - 3; i++)
            {
                uniqueID = BitConverter.ToUInt32(data, i);
            }

            var filepath = Path.Combine(Application.dataPath, "kiwiManagerSave_" + uniqueID + ".xml");
            Log.Message("Loading from " + filepath);
            _timer.Enabled = false;

            if (!File.Exists(filepath))
            {
                Log.Warning("Serialized data file does not exist!");
                return;
            }

            Configuration configuration = Configuration.Deserialize(filepath);

            bool[] prinodes = new bool[NetManager.MAX_NODE_COUNT];
            for (int i = 0; i < configuration.prioritySegments.Count; ++i)
            {
                if (configuration.prioritySegments[i][2] == 0)
                {
                    continue;
                }
                if (!TrafficPriority.isPrioritySegment(
                        (ushort) configuration.prioritySegments[i][0],
                        (ushort) configuration.prioritySegments[i][1]))
                {
                    TrafficPriority.addPrioritySegment(
                        (ushort) configuration.prioritySegments[i][0],
                        (ushort) configuration.prioritySegments[i][1],
                        (PrioritySegment.PriorityType) configuration.prioritySegments[i][2]);
                    prinodes[configuration.prioritySegments[i][0]] = true;
                }
                else
                {
                    Log.Warning(((ushort) configuration.prioritySegments[i][0]) + ">" + ((ushort) configuration.prioritySegments[i][1]) + ">" +
                            " has already been added as a priority segment!");
                }
            }
            for (ushort nodeID = 0; nodeID < prinodes.Length; ++nodeID)
            {
                if (prinodes[nodeID])
                {
                    for (int j = 0; j < 8; ++j)
                    {
                        ushort segmentID = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].GetSegment(j);
                        if (!TrafficPriority.isPrioritySegment(nodeID, segmentID))
                        {
                            TrafficPriority.addPrioritySegment(nodeID, segmentID, PrioritySegment.PriorityType.None);
                        }
                    }
                }
            }

            for (int i = 0; i < configuration.nodeDictionary.Count; ++i)
            {
                if (CustomRoadAI.GetNodeSimulation((ushort)configuration.nodeDictionary[i][0]) == null)
                {
                    CustomRoadAI.AddNodeToSimulation((ushort)configuration.nodeDictionary[i][0]);
                    var nodeDict = CustomRoadAI.GetNodeSimulation((ushort)configuration.nodeDictionary[i][0]);

                    nodeDict._manualTrafficLights = Convert.ToBoolean(configuration.nodeDictionary[i][1]);
                    nodeDict._timedTrafficLights = Convert.ToBoolean(configuration.nodeDictionary[i][2]);
                    nodeDict.TimedTrafficLightsActive = Convert.ToBoolean(configuration.nodeDictionary[i][3]);
                }
            }

            for (int i = 0; i < configuration.manualSegments.Count; ++i)
            {
                var segmentData = configuration.manualSegments[i];

                if (!TrafficLightsManual.IsSegmentLight((ushort)segmentData[0], segmentData[1]))
                {
                    TrafficLightsManual.AddSegmentLight((ushort)segmentData[0], segmentData[1],
                        RoadBaseAI.TrafficLightState.Green);
                    var segment = TrafficLightsManual.GetSegmentLight((ushort)segmentData[0], segmentData[1]);
                    segment.currentMode = (ManualSegmentLight.Mode)segmentData[2];
                    segment.lightLeft = (RoadBaseAI.TrafficLightState)segmentData[3];
                    segment.lightMain = (RoadBaseAI.TrafficLightState)segmentData[4];
                    segment.lightRight = (RoadBaseAI.TrafficLightState)segmentData[5];
                    segment.lightPedestrian = (RoadBaseAI.TrafficLightState)segmentData[6];
                    segment.lastChange = (uint)segmentData[7];
                    segment.lastChangeFrame = (uint)segmentData[8];
                    segment.pedestrianEnabled = Convert.ToBoolean(segmentData[9]);
                }
            }

            var timedStepCount = 0;
            var timedStepSegmentCount = 0;

            Log.Message("got to timed nodes");
            {
            int q_i = -1;
            int q_j = -1;
            int q_k = -1;
            ushort q_nodeid = 0;
            try {
            for (var i = 0; i < configuration.timedNodes.Count; i++)
            {
                q_i = i;
                q_j = -1;
                q_k = -1;
                var nodeid = (ushort)configuration.timedNodes[i][0];
                q_nodeid = nodeid;

                var nodeGroup = new List<ushort>();
                for (var j = 0; j < configuration.timedNodeGroups[i].Length; j++)
                {
                    q_j = j;
                    nodeGroup.Add(configuration.timedNodeGroups[i][j]);
                }
                q_j = -2;

                if (!TrafficLightsTimed.IsTimedLight(nodeid))
                {
                    TrafficLightsTimed.AddTimedLight(nodeid, nodeGroup);
                    var timedNode = TrafficLightsTimed.GetTimedLight(nodeid);

                    timedNode.currentStep = configuration.timedNodes[i][1];

                    for (var j = 0; j < configuration.timedNodes[i][2]; j++)
                    {
                        q_j = j;
                        var cfgstep = configuration.timedNodeSteps[timedStepCount];

                        timedNode.addStep(cfgstep[0]);

                        var step = timedNode.steps[j];

                        for (var k = 0; k < cfgstep[1]; k++)
                        {
                            q_k = k;
                            step.lightLeft[k] = (RoadBaseAI.TrafficLightState)configuration.timedNodeStepSegments[timedStepSegmentCount][0];
                            step.lightMain[k] = (RoadBaseAI.TrafficLightState)configuration.timedNodeStepSegments[timedStepSegmentCount][1];
                            step.lightRight[k] = (RoadBaseAI.TrafficLightState)configuration.timedNodeStepSegments[timedStepSegmentCount][2];
                            step.lightPedestrian[k] = (RoadBaseAI.TrafficLightState)configuration.timedNodeStepSegments[timedStepSegmentCount][3];

                            timedStepSegmentCount++;
                        }
                        q_k = -2;

                        timedStepCount++;
                    }
                    q_j = -3;

                    if (Convert.ToBoolean(configuration.timedNodes[i][3]))
                    {
                        timedNode.start();
                    }
                }
            }
            } catch (Exception ex) {
                Log.Error(ex.ToString());
                Log.Warning("i: " + q_i);
                Log.Warning("j: " + q_j);
                Log.Warning("k: " + q_k);
                Log.Warning("nodeid: " + q_nodeid);
                Log.Warning("timedStepCount: " + timedStepCount);
                Log.Warning("timedStepSegmentCount: " + timedStepSegmentCount);
                Log.Warning("TimedTrafficSteps.num: " + TimedTrafficSteps.s_num);
                Log.Warning("TimedTrafficSteps.nodeID: " + TimedTrafficSteps.s_nodeID);
                Log.Warning("TimedTrafficSteps.segment: " + TimedTrafficSteps.s_segment);
                Log.Warning("TimedTrafficSteps.node?: " + TimedTrafficSteps.s_node.HasValue);
                Log.Warning("TimedTrafficSteps.segmentLight: " + TimedTrafficSteps.s_segmentLight);
            }
            }

            Log.Message("got to traffic light flags");
            for (int index = 0, nodeID = 0; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID)
            {
                if (Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_flags != 0 &&
                    Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].Info.m_class.m_service == ItemClass.Service.Road)
                {
                    if (configuration.nodeTrafficLights[index] == '1')
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_flags |= NetNode.Flags.TrafficLights;
                    }
                    else
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_flags &= ~NetNode.Flags.TrafficLights;
                    }
                    if (configuration.nodeCrosswalk[index] == '1')
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_flags |= NetNode.Flags.Junction;
                    }
                    else
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_flags &= ~NetNode.Flags.Junction;
                    }
                    ++index;
                }
            }

            NetSegment[] segments = Singleton<NetManager>.instance.m_segments.m_buffer;
            NetLane[] lanes = Singleton<NetManager>.instance.m_lanes.m_buffer;

            string[] lanespecs = configuration.laneFlags.Split(',');

            Log.Message("got to lanes");
            bool[] ttouched = new bool[NetManager.MAX_SEGMENT_COUNT];
            foreach (string lanespec in lanespecs)
            {
                string[] split = lanespec.Split(':');
                int laneid = Convert.ToInt32(split[0]);
                if ((lanes[laneid].m_flags & (ushort) NetLane.Flags.Created) == 0) continue;
//                lanes[laneid].m_flags = Convert.ToUInt16(split[1]);
                if (split.Length >= 4)
                {
                    if (split.Length >= 5 && Convert.ToUInt16(split[4]) != lanes[laneid].m_segment)
                    {
                        Log.Warning(String.Format("laneID {0} has segmentID {1} but file says segmentID {2}",
                                laneid, lanes[laneid].m_segment, Convert.ToUInt16(split[4])));
                        continue;
                    }
                    if (lanes[laneid].m_firstTarget != Convert.ToByte(split[2]) || lanes[laneid].m_lastTarget != Convert.ToByte(split[3]))
                    {
                        NetInfo info = segments[lanes[laneid].m_segment].Info;
                        uint olane = segments[lanes[laneid].m_segment].m_lanes;
                        int lindex = 0;
                        while (olane != laneid && olane != 0 && lindex < info.m_lanes.Length)
                        {
                            olane = lanes[olane].m_nextLane;
                            ++lindex;
                        }
                        if (olane != laneid)
                        {
                            Log.Warning(String.Format("couldn't find laneID {0} in segmentID {1} lanes", laneid, lanes[laneid].m_segment));
                            if (olane != 0) Log.Warning(String.Format("ran out of lanes in {0}!", info.name));
                            continue;
                        }
                        if (info.m_lanes[lindex].m_laneType != NetInfo.LaneType.Vehicle)
                        {
                            Log.Warning(String.Format("laneID {0} is not a Vehicle lane (it's {1})", info.m_lanes[lindex].m_laneType));
                            continue;
                        }
                    }
                    byte firstTarget = Convert.ToByte(split[2]);
                    if (lanes[laneid].m_firstTarget != firstTarget)
                    {
                        lanes[laneid].m_firstTarget = firstTarget;
                        ttouched[lanes[laneid].m_segment] = true;
                    }
                    byte lastTarget = Convert.ToByte(split[3]);
                    if (lanes[laneid].m_lastTarget != lastTarget)
                    {
                        lanes[laneid].m_lastTarget = lastTarget;
                        ttouched[lanes[laneid].m_segment] = true;
                    }
                }
            }
            for (ushort segmentID = 0; segmentID < ttouched.Length; ++segmentID)
            {
                if (ttouched[segmentID])
                {
                    TrafficLightTool.UpdateLaneFlags(segments[segmentID].m_startNode, segmentID);
                    TrafficLightTool.UpdateLaneFlags(segments[segmentID].m_endNode, segmentID);
                }
            }
        }

        public void OnSaveData()
        {
            NetNode[] nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            NetSegment[] segments = Singleton<NetManager>.instance.m_segments.m_buffer;
            NetLane[] lanes = Singleton<NetManager>.instance.m_lanes.m_buffer;

            FastList<byte> data = new FastList<byte>();

            GenerateUniqueID(); 

            byte[] uniqueIdBytes = BitConverter.GetBytes(uniqueID);
            foreach (byte uniqueIdByte in uniqueIdBytes)
            {
                data.Add(uniqueIdByte);
            }

            byte[] dataToSave = data.ToArray();
            SerializableData.SaveData(dataID, dataToSave);

            string filepath = Path.Combine(Application.dataPath, "kiwiManagerSave_" + uniqueID + ".xml");

            Configuration configuration = new Configuration();

            foreach (PrioritySegment priseg in TrafficPriority.prioritySegments.Values)
            {
                if (priseg.nodeid == segments[priseg.segmentid].m_startNode ||
                    priseg.nodeid == segments[priseg.segmentid].m_endNode)
                {
                    configuration.prioritySegments.Add(new int[3] { priseg.nodeid, priseg.segmentid, (int) priseg.type });
                }
            }

            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID)
            {
                if (CustomRoadAI.nodeDictionary.ContainsKey(segmentID))
                {
                    var nodeDict = CustomRoadAI.nodeDictionary[segmentID];

                    configuration.nodeDictionary.Add(new int[4] {nodeDict.NodeId, Convert.ToInt32(nodeDict._manualTrafficLights), Convert.ToInt32(nodeDict._timedTrafficLights), Convert.ToInt32(nodeDict.TimedTrafficLightsActive)});
                }

                if (TrafficLightsManual.ManualSegments.ContainsKey(segmentID))
                {
                    if (TrafficLightsManual.ManualSegments[segmentID].node_1 != 0)
                    {
                        ManualSegmentLight manualSegment = TrafficLightsManual.ManualSegments[segmentID].instance_1;

                        configuration.manualSegments.Add(new int[10]
                        {
                            (int)manualSegment.node,
                            manualSegment.segment,
                            (int)manualSegment.currentMode,
                            (int)manualSegment.lightLeft,
                            (int)manualSegment.lightMain,
                            (int)manualSegment.lightRight,
                            (int)manualSegment.lightPedestrian,
                            (int)manualSegment.lastChange,
                            (int)manualSegment.lastChangeFrame,
                            Convert.ToInt32(manualSegment.pedestrianEnabled)
                        });
                    }
                    if (TrafficLightsManual.ManualSegments[segmentID].node_2 != 0)
                    {
                        ManualSegmentLight manualSegment = TrafficLightsManual.ManualSegments[segmentID].instance_2;

                        configuration.manualSegments.Add(new int[10]
                        {
                            (int)manualSegment.node,
                            manualSegment.segment,
                            (int)manualSegment.currentMode,
                            (int)manualSegment.lightLeft,
                            (int)manualSegment.lightMain,
                            (int)manualSegment.lightRight,
                            (int)manualSegment.lightPedestrian,
                            (int)manualSegment.lastChange,
                            (int)manualSegment.lastChangeFrame,
                            Convert.ToInt32(manualSegment.pedestrianEnabled)
                        });
                    }
                }

                if (TrafficLightsTimed.timedScripts.ContainsKey(segmentID))
                {
                    var timedNode = TrafficLightsTimed.GetTimedLight(segmentID);

                    configuration.timedNodes.Add(new int[4] { timedNode.nodeID, timedNode.currentStep, timedNode.NumSteps(), Convert.ToInt32(timedNode.isStarted())});

                    var nodeGroup = new ushort[timedNode.nodeGroup.Count];

                    for (var j = 0; j < timedNode.nodeGroup.Count; j++)
                    {
                        nodeGroup[j] = timedNode.nodeGroup[j];
                    }

                    configuration.timedNodeGroups.Add(nodeGroup);

                    for (var j = 0; j < timedNode.NumSteps(); j++)
                    {
                        configuration.timedNodeSteps.Add(new int[2]
                        {
                            timedNode.steps[j].numSteps,
                            timedNode.steps[j].segments.Count
                        });

                        for (var k = 0; k < timedNode.steps[j].segments.Count; k++)
                        {
                            configuration.timedNodeStepSegments.Add(new int[4]
                            {
                                (int)timedNode.steps[j].lightLeft[k],
                                (int)timedNode.steps[j].lightMain[k],
                                (int)timedNode.steps[j].lightRight[k],
                                (int)timedNode.steps[j].lightPedestrian[k],
                            });
                        }
                    }
                }
            }

            for (ushort nodeID = 0; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID)
            {
                NetNode.Flags nodeFlags = nodes[nodeID].m_flags;
                if (nodeFlags != 0 &&
                    nodes[nodeID].Info.m_class.m_service == ItemClass.Service.Road)
                {
                    configuration.nodeTrafficLights += ((nodeFlags & NetNode.Flags.TrafficLights) != 0) ? '1' : '0';
                    configuration.nodeCrosswalk += ((nodeFlags & NetNode.Flags.Junction) != 0) ? '1' : '0';
                }
            }

            for (uint laneID = 0; laneID < NetManager.MAX_LANE_COUNT; ++laneID)
            {
                ushort segmentID = lanes[laneID].m_segment;
                if (TrafficPriority.isPrioritySegment(segments[segmentID].m_startNode, segmentID) ||
                    TrafficPriority.isPrioritySegment(segments[segmentID].m_endNode, segmentID))
                {
                    configuration.laneFlags += string.Format("{0}:{1}:{2}:{3}:{4},", laneID,
                            lanes[laneID].m_flags,
                            lanes[laneID].m_firstTarget,
                            lanes[laneID].m_lastTarget,
                            lanes[laneID].m_segment);
                }
            }

            Configuration.Serialize(filepath, configuration);
        }
    }

    public class Configuration
    {
        public string nodeTrafficLights;
        public string nodeCrosswalk;
        public string laneFlags;

        public List<int[]> prioritySegments = new List<int[]>();
        public List<int[]> nodeDictionary = new List<int[]>(); 
        public List<int[]> manualSegments = new List<int[]>(); 

        public List<int[]> timedNodes = new List<int[]>();
        public List<ushort[]> timedNodeGroups = new List<ushort[]>();
        public List<int[]> timedNodeSteps = new List<int[]>();
        public List<int[]> timedNodeStepSegments = new List<int[]>(); 

        public void OnPreSerialize()
        {
        }

        public void OnPostDeserialize()
        {
        }

        public static void Serialize(string filename, Configuration config)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            using (var writer = new StreamWriter(filename))
            {
                config.OnPreSerialize();
                serializer.Serialize(writer, config);
            }
        }

        public static Configuration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    var config = (Configuration)serializer.Deserialize(reader);
                    config.OnPostDeserialize();
                    return config;
                }
            }
            catch { }

            return null;
        }
    }
}
