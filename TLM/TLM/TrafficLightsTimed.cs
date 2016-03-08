﻿using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework;
using UnityEngine;

namespace KiwiManager
{
    public class TimedTrafficSteps : ICloneable
    {
        public static int s_num = -1;
        public static ushort s_nodeID = 0;
        public static ushort s_segment = 0;
        public static NetNode? s_node = null;
        public static ManualSegmentLight s_segmentLight = null;

        public ushort nodeID;
        public int numSteps;
        public uint frame;

        public List<int> segments = new List<int>();

        public List<RoadBaseAI.TrafficLightState> lightMain = new List<RoadBaseAI.TrafficLightState>();
        public List<RoadBaseAI.TrafficLightState> lightLeft = new List<RoadBaseAI.TrafficLightState>();
        public List<RoadBaseAI.TrafficLightState> lightRight = new List<RoadBaseAI.TrafficLightState>();
        public List<RoadBaseAI.TrafficLightState> lightPedestrian = new List<RoadBaseAI.TrafficLightState>(); 

        public TimedTrafficSteps(int num, ushort nodeID)
        {
            s_num = num;
            s_nodeID = nodeID;
            this.nodeID = nodeID;
            this.numSteps = num;

            NetNode node = TrafficLightTool.GetNetNode(nodeID);
            s_node = node;

            for (int s = 0; s < 8; ++s)
            {
                ushort segment = node.GetSegment(s);
                if (segment != 0)
                {
                    segments.Add(segment);
                }
            }
            try
            {
                foreach (ushort segment in segments)
                {
                    s_segment = segment;
                    ManualSegmentLight segmentLight = TrafficLightsManual.GetSegmentLight(nodeID, segment);
                    s_segmentLight = segmentLight;

                    lightMain.Add(segmentLight.GetLightMain());
                    lightLeft.Add(segmentLight.GetLightLeft());
                    lightRight.Add(segmentLight.GetLightRight());
                    lightPedestrian.Add(segmentLight.GetLightPedestrian());
                    s_segmentLight = null;
                }
            }
            catch (NullReferenceException nre)
            {
                string s = "[";
                foreach (ushort segment in segments)
                {
                    s += segment;
                    s += ',';
                }
                s += "]";
                Log.Error("null reference, " + s + " (trying to get " + s_segment + ")");
                throw nre;
            }
            s_num = -1;
            s_nodeID = 0;
            s_node = null;
        }

        public RoadBaseAI.TrafficLightState getLight(int segment, int lightType)
        {
            for (var i = 0; i < segments.Count; i++)
            {
                if (segments[i] == segment)
                {
                    if (lightType == 0)
                        return lightMain[i];
                    else if (lightType == 1)
                        return lightLeft[i];
                    else if (lightType == 2)
                        return lightRight[i];
                    else if (lightType == 3)
                        return lightPedestrian[i];
                }
            }

            return RoadBaseAI.TrafficLightState.Green;
        }

        public void setFrame(uint frame)
        {
            this.frame = frame;
        }

        public void setLights()
        {
            for (int s = 0; s < segments.Count; s++)
            {
                var segment = segments[s];

                if (segment != 0)
                {
                    var segmentLight = TrafficLightsManual.GetSegmentLight(nodeID, segment);

                    segmentLight.lightMain = lightMain[s];
                    segmentLight.lightLeft = lightLeft[s];
                    segmentLight.lightRight = lightRight[s];
                    segmentLight.lightPedestrian = lightPedestrian[s];
                    segmentLight.UpdateVisuals();
                }
            }
        }

        public void updateLights()
        {
            for (int s = 0; s < segments.Count; s++)
            {
                var segment = segments[s];

                if (segment != 0)
                {
                    var segmentLight = TrafficLightsManual.GetSegmentLight(nodeID, segment);

                    lightMain[s] = segmentLight.lightMain;
                    lightLeft[s] = segmentLight.lightLeft;
                    lightRight[s] = segmentLight.lightRight;
                    lightPedestrian[s] = segmentLight.lightPedestrian;
                }
            }
        }

        public long currentStep()
        {
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            return this.frame + this.numSteps - (currentFrameIndex >> 6);
        }

        public bool stepDone(uint frame)
        {
            if (this.frame + this.numSteps <= frame)
            {
                return true;
            }

            return false;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class TrafficLightsTimed
    {
        public ushort nodeID;

        public static Dictionary<ushort, TrafficLightsTimed> timedScripts = new Dictionary<ushort, TrafficLightsTimed>(); 

        public List<TimedTrafficSteps> steps = new List<TimedTrafficSteps>();
        public int currentStep = 0;

        public List<ushort> nodeGroup; 

        public TrafficLightsTimed(ushort nodeID, List<ushort> nodeGroup)
        {
            this.nodeID = nodeID;
            this.nodeGroup = new List<ushort>(nodeGroup);

            CustomRoadAI.GetNodeSimulation(nodeID).TimedTrafficLightsActive = false;
        }

        public void addStep(int num)
        {
            steps.Add(new TimedTrafficSteps(num, nodeID));
        }

        public void start()
        {
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            currentStep = 0;
            steps[0].setLights();
            steps[0].setFrame(currentFrameIndex >> 6);

            CustomRoadAI.GetNodeSimulation(nodeID).TimedTrafficLightsActive = true;
        }

        public void moveStep(int oldPos , int newPos )
        {
            TimedTrafficSteps oldStep = steps[oldPos];

            steps.RemoveAt(oldPos);
            steps.Insert(newPos, oldStep);
        }

        public void stop()
        {
            CustomRoadAI.GetNodeSimulation(nodeID).TimedTrafficLightsActive = false;
        }

        public bool isStarted()
        {
            return CustomRoadAI.GetNodeSimulation(nodeID).TimedTrafficLightsActive;
        }

        public int NumSteps()
        {
            return steps.Count;
        }

        public TimedTrafficSteps GetStep(int stepID)
        {
            return steps[stepID];
        }

        public void checkStep(uint frame)
        {
            if (steps[currentStep].stepDone(frame))
            {
                currentStep = currentStep + 1 >= steps.Count ? 0 : currentStep + 1;

                steps[currentStep].setFrame(frame);
                steps[currentStep].setLights();
            }
        }

        public void skipStep()
        {
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            currentStep = currentStep + 1 >= steps.Count ? 0 : currentStep + 1;

            steps[currentStep].setFrame(currentFrameIndex >> 6);
            steps[currentStep].setLights();
        }

        public long checkNextChange(int segmentID, int lightType)
        {
            var startStep = currentStep;
            var stepNum = currentStep + 1;
            var numFrames = steps[currentStep].currentStep();

            RoadBaseAI.TrafficLightState currentState;

            if (lightType == 0)
                currentState = TrafficLightsManual.GetSegmentLight(this.nodeID, segmentID).GetLightMain();
            else if (lightType == 1)
                currentState = TrafficLightsManual.GetSegmentLight(this.nodeID, segmentID).GetLightLeft();
            else if (lightType == 2)
                currentState = TrafficLightsManual.GetSegmentLight(this.nodeID, segmentID).GetLightRight();
            else
                currentState = TrafficLightsManual.GetSegmentLight(this.nodeID, segmentID).GetLightPedestrian();


            while (true)
            {
                if (stepNum > NumSteps() - 1)
                {
                    stepNum = 0;
                }

                if (stepNum == startStep)
                {
                    numFrames = 99;
                    break;
                }

                var light = steps[stepNum].getLight(segmentID, lightType);

                if (light != currentState)
                {
                    break;
                }
                else
                {
                    numFrames += steps[stepNum].numSteps;
                }

                stepNum++;
            }

            return numFrames;
        }

        public void resetSteps()
        {
            steps.Clear();
        }

        public void RemoveStep(int id)
        {
            steps.RemoveAt(id);
        }

        public static void AddTimedLight(ushort nodeid, List<ushort> nodeGroup)
        {
            timedScripts.Add(nodeid, new TrafficLightsTimed(nodeid, nodeGroup));
        }

        public static void RemoveTimedLight(ushort nodeid)
        {
            timedScripts.Remove(nodeid);
        }

        public static bool IsTimedLight(ushort nodeid)
        {
            return timedScripts.ContainsKey(nodeid);
        }

        public static TrafficLightsTimed GetTimedLight(ushort nodeid)
        {
            return timedScripts[nodeid];
        }
    }
}
