using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace KiwiManager
{
    public class UITrafficManager : UIPanel
    {
        public enum UIState
        {
            None,
            SwitchTrafficLight,
            AddStopSign,
            ManualSwitch,
            TimedControlNodes,
            TimedControlLights,
            LaneChange,
            LaneRestrictions,
            Crosswalk
        }

        private static UIState _uistate = UIState.None;

        private static bool inited = false;

        public static UIState uistate
        {
            set
            {
                if (value == UIState.None && inited)
                {
                    buttonSwitchTraffic.focusedBgSprite = "ButtonMenu";
                    buttonPrioritySigns.focusedBgSprite = "ButtonMenu";
                    buttonManualControl.focusedBgSprite = "ButtonMenu";
                    buttonTimedMain.focusedBgSprite = "ButtonMenu";

                    
                    //buttonLaneRestrictions.focusedBgSprite = "ButtonMenu";
                    buttonCrosswalk.focusedBgSprite = "ButtonMenu";
                    buttonClearTraffic.focusedBgSprite = "ButtonMenu";
                    if (!LoadingExtension.PathfinderIncompatibility)
                    {
                        buttonLaneChange.focusedBgSprite = "ButtonMenu";
                        buttonToggleDespawn.focusedBgSprite = "ButtonMenu";
                    }
                }

                _uistate = value;
            }
            get { return _uistate; }
        }

        private static UIButton buttonSwitchTraffic;
        private static UIButton buttonPrioritySigns;
        private static UIButton buttonManualControl;
        private static UIButton buttonTimedMain;
        private static UIButton buttonLaneChange;
        private static UIButton buttonLaneRestrictions;
        private static UIButton buttonCrosswalk;
        private static UIButton buttonClearTraffic;
        private static UIButton buttonToggleDespawn;
        private static UIButton buttonDumpSVG;

        public static TrafficLightTool trafficLightTool;

        public override void Start()
        {
            inited = true;

            trafficLightTool = LoadingExtension.Instance.TrafficLightTool;

            this.backgroundSprite = "GenericPanel";
            this.color = new Color32(75, 75, 135, 255);
            this.width = 250;
            this.height = !LoadingExtension.PathfinderIncompatibility ? 390 : 310;
            this.relativePosition = new Vector3(10.48f, 80f);

            UILabel title = this.AddUIComponent<UILabel>();
            title.text = "Traffic Manager";
            title.relativePosition = new Vector3(65.0f, 5.0f);

            if (!LoadingExtension.PathfinderIncompatibility)
            {
                buttonSwitchTraffic = _createButton("Switch traffic lights", new Vector3(35f, 30f), clickSwitchTraffic);
                buttonPrioritySigns = _createButton("Add priority signs", new Vector3(35f, 70f), clickAddPrioritySigns);
                buttonManualControl = _createButton("Manual traffic lights", new Vector3(35f, 110f), clickManualControl);
                buttonTimedMain = _createButton("Timed traffic lights", new Vector3(35f, 150f), clickTimedAdd);
                buttonLaneChange = _createButton("Change lanes", new Vector3(35f, 190f), clickChangeLanes);
                //buttonLaneRestrictions = _createButton("Road Restrictions", new Vector3(35f, 230f), clickLaneRestrictions);
                buttonCrosswalk = _createButton("Add/Remove Crosswalk", new Vector3(35f, 230f), clickCrosswalk);
                buttonClearTraffic = _createButton("Clear Traffic", new Vector3(35f, 270f), clickClearTraffic);
                buttonToggleDespawn = _createButton(LoadingExtension.Instance.despawnEnabled ? "Disable despawning" : "Enable despawning", new Vector3(35f, 310f), clickToggleDespawn);
                buttonDumpSVG = _createButton("Export to SVG", new Vector3(35f, 350f), clickDumpSVG);

            }
            else
            {
                buttonSwitchTraffic = _createButton("Switch traffic lights", new Vector3(35f, 30f), clickSwitchTraffic);
                buttonPrioritySigns = _createButton("Add priority signs", new Vector3(35f, 70f), clickAddPrioritySigns);
                buttonManualControl = _createButton("Manual traffic lights", new Vector3(35f, 110f), clickManualControl);
                buttonTimedMain = _createButton("Timed traffic lights", new Vector3(35f, 150f), clickTimedAdd);
                buttonCrosswalk = _createButton("Add/Remove Crosswalk", new Vector3(35f, 190f), clickCrosswalk);
                buttonClearTraffic = _createButton("Clear Traffic", new Vector3(35f, 230f), clickClearTraffic);
                buttonDumpSVG = _createButton("Export to SVG", new Vector3(35f, 270f), clickDumpSVG);
            }
        }

        private UIButton _createButton(string text, Vector3 pos, MouseEventHandler eventClick)
        {
            var button = this.AddUIComponent<UIButton>();
            button.width = 190;
            button.height = 30;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.playAudioEvents = true;
            button.text = text;
            button.relativePosition = pos;
            button.eventClick += eventClick;

            return button;
        }

        private void clickSwitchTraffic(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.SwitchTrafficLight)
            {
                _uistate = UIState.SwitchTrafficLight;

                buttonSwitchTraffic.focusedBgSprite = "ButtonMenuFocused";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.SwitchTrafficLight);
            }
            else
            {
                _uistate = UIState.None;

                buttonSwitchTraffic.focusedBgSprite = "ButtonMenu";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void clickAddPrioritySigns(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.AddStopSign)
            {
                _uistate = UIState.AddStopSign;

                buttonPrioritySigns.focusedBgSprite = "ButtonMenuFocused";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.AddPrioritySigns);
            }
            else
            {
                _uistate = UIState.None;

                buttonPrioritySigns.focusedBgSprite = "ButtonMenu";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void clickManualControl(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.ManualSwitch)
            {
                _uistate = UIState.ManualSwitch;

                buttonManualControl.focusedBgSprite = "ButtonMenuFocused";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.ManualSwitch);
            }
            else
            {
                _uistate = UIState.None;

                buttonManualControl.focusedBgSprite = "ButtonMenu";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void clickTimedAdd(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.TimedControlNodes)
            {
                _uistate = UIState.TimedControlNodes;

                buttonTimedMain.focusedBgSprite = "ButtonMenuFocused";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.TimedLightsSelectNode);
            }
            else
            {
                _uistate = UIState.None;

                buttonTimedMain.focusedBgSprite = "ButtonMenu";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void clickClearTraffic(UIComponent component, UIMouseEventParameter eventParam)
        {
            List<ushort> vehicleList = new List<ushort>();

            foreach (var vehicleID in TrafficPriority.vehicleList.Keys)
            {
                vehicleList.Add(vehicleID);
            }

            lock (Singleton<VehicleManager>.instance)
            {
                for (var i = 0; i < vehicleList.Count; i++)
                {
                    var vehicleData = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleList[i]];

                    if (vehicleData.Info.m_vehicleType == VehicleInfo.VehicleType.Car)
                    {
                        Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleList[i]);
                    }
                }
            }
        }

        private void clickToggleDespawn(UIComponent component, UIMouseEventParameter eventParam)
        {
            LoadingExtension.Instance.despawnEnabled = !LoadingExtension.Instance.despawnEnabled;

            if (!LoadingExtension.PathfinderIncompatibility)
            {
                buttonToggleDespawn.text = LoadingExtension.Instance.despawnEnabled
                    ? "Disable despawning"
                    : "Enable despawning";
            }
        }

        private void clickChangeLanes(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.LaneChange)
            {
                _uistate = UIState.LaneChange;

                if (!LoadingExtension.PathfinderIncompatibility)
                {
                    buttonLaneChange.focusedBgSprite = "ButtonMenuFocused";
                }

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.LaneChange);
            }
            else
            {
                _uistate = UIState.None;

                if (!LoadingExtension.PathfinderIncompatibility)
                {
                    buttonLaneChange.focusedBgSprite = "ButtonMenu";
                }

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void clickLaneRestrictions(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.LaneRestrictions)
            {
                _uistate = UIState.LaneRestrictions;

                buttonLaneRestrictions.focusedBgSprite = "ButtonMenuFocused";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.LaneRestrictions);
            }
            else
            {
                _uistate = UIState.None;

                buttonLaneRestrictions.focusedBgSprite = "ButtonMenu";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void clickCrosswalk(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_uistate != UIState.Crosswalk)
            {
                _uistate = UIState.Crosswalk;

                buttonCrosswalk.focusedBgSprite = "ButtonMenuFocused";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.Crosswalk);
            }
            else
            {
                _uistate = UIState.None;

                buttonCrosswalk.focusedBgSprite = "ButtonMenu";

                TrafficLightTool.setToolMode(TrafficLightTool.ToolMode.None);
            }
        }

        private void writeCoord(Vector3 coord, StreamWriter writer)
        {
            writer.Write(" {0,7:F1} {1,7:F1}", coord.x, -coord.z);
        }
        private void writeCoord(Vector2 coord, StreamWriter writer)
        {
            writer.Write(" {0,7:F1} {1,7:F1}", coord.x, coord.y);
        }
        private void writePath(StreamWriter writer, Vector2 p0, Vector2? p1, Vector2? p2, Vector2 p3)
        {
            writer.Write("M");
            writeCoord(p0, writer);
            if (p1 != null && p2 != null)
            {
                writer.Write(" C");
                writeCoord(p1.Value, writer);
                writeCoord(p2.Value, writer);
            }
            else
            {
                writer.Write("                 L                ");
            }
            writeCoord(p3, writer);
        }
        private bool outOfBounds(Vector3 start, Vector3 end)
        {
            return !(start.x >= -1920 && start.x <= 3840 ||
                start.y >= -1920 && start.y <= 1920 ||
                end.x >= -1920 && end.x <= 3840 ||
                end.y >= -1920 && end.y <= 1920);
        }
        private void dumpNetInfo(NetInfo ni, StreamWriter writer)
        {
            writer.Write("<NetInfo class=\"{0}\"", ni.name);
            writer.Write(" {0}=\"{1}\"", "m_halfWidth", ni.m_halfWidth);
            writer.Write(" {0}=\"{1}\"", "m_pavementWidth", ni.m_pavementWidth);
            writer.Write(" {0}=\"{1}\"", "m_segmentLength", ni.m_segmentLength);
            writer.Write(" {0}=\"{1}\"", "m_minHeight", ni.m_minHeight);
            writer.Write(" {0}=\"{1}\"", "m_maxHeight", ni.m_maxHeight);
            writer.Write(" {0}=\"{1}\"", "m_maxSlope", ni.m_maxSlope);
            writer.Write(" {0}=\"{1}\"", "m_maxBuildAngle", ni.m_maxBuildAngle);
            writer.Write(" {0}=\"{1}\"", "m_maxTurnAngle", ni.m_maxTurnAngle);
            writer.Write(" {0}=\"{1}\"", "m_minCornerOffset", ni.m_minCornerOffset);
            writer.Write(" {0}=\"{1}\"", "m_buildHeight", ni.m_buildHeight);
            writer.Write(" {0}=\"{1}\"", "m_surfaceLevel", ni.m_surfaceLevel);
            writer.Write(" {0}=\"{1}\"", "m_createPavement", ni.m_createPavement);
            writer.Write(" {0}=\"{1}\"", "m_createGravel", ni.m_createGravel);
            writer.Write(" {0}=\"{1}\"", "m_createRuining", ni.m_createRuining);
            writer.Write(" {0}=\"{1}\"", "m_flattenTerrain", ni.m_flattenTerrain);
            writer.Write(" {0}=\"{1}\"", "m_lowerTerrain", ni.m_lowerTerrain);
            writer.Write(" {0}=\"{1}\"", "m_clipTerrain", ni.m_clipTerrain);
            writer.Write(" {0}=\"{1}\"", "m_followTerrain", ni.m_followTerrain);
            writer.Write(" {0}=\"{1}\"", "m_flatJunctions", ni.m_flatJunctions);
            writer.Write(" {0}=\"{1}\"", "m_snapBuildingNodes", ni.m_snapBuildingNodes);
            writer.Write(" {0}=\"{1}\"", "m_canDisable", ni.m_canDisable);
            writer.Write(" {0}=\"{1:F}\"", "m_setVehicleFlags", ni.m_setVehicleFlags);
            writer.Write(" {0}=\"{1}\"", "m_maxBuildAngleCos", ni.m_maxBuildAngleCos);
            writer.Write(" {0}=\"{1}\"", "m_maxTurnAngleCos", ni.m_maxTurnAngleCos);
            writer.Write(" {0}=\"{1}\"", "m_hasForwardVehicleLanes", ni.m_hasForwardVehicleLanes);
            writer.Write(" {0}=\"{1}\"", "m_hasBackwardVehicleLanes", ni.m_hasBackwardVehicleLanes);
            writer.Write(" {0}=\"{1}\"", "m_averageVehicleLaneSpeed", ni.m_averageVehicleLaneSpeed);
            writer.Write(" {0}=\"{1}\"", "m_hasParkingSpaces", ni.m_hasParkingSpaces);
            writer.Write(" {0}=\"{1}\"", "m_treeLayers", ni.m_treeLayers);
            writer.Write(" {0}=\"{1}\"", "m_netLayers", ni.m_netLayers);
            writer.Write(" {0}=\"{1}\"", "m_maxPropDistance", ni.m_maxPropDistance);
            writer.WriteLine(">");
            foreach (NetInfo.Lane lane in ni.m_lanes)
            {
                writer.Write("\t<Lane");
                writer.Write(" {0}=\"{1}\"", "m_position", lane.m_position);
                writer.Write(" {0}=\"{1}\"", "m_width", lane.m_width);
                writer.Write(" {0}=\"{1}\"", "m_verticalOffset", lane.m_verticalOffset);
                writer.Write(" {0}=\"{1}\"", "m_stopOffset", lane.m_stopOffset);
                writer.Write(" {0}=\"{1}\"", "m_speedLimit", lane.m_speedLimit);
                writer.Write(" {0}=\"{1:F}\"", "m_direction", lane.m_direction);
                writer.Write(" {0}=\"{1:F}\"", "m_laneType", lane.m_laneType);
                writer.Write(" {0}=\"{1:F}\"", "m_vehicleType", lane.m_vehicleType);
                writer.Write(" {0}=\"{1:F}\"", "m_laneProps", lane.m_laneProps);
                writer.Write(" {0}=\"{1}\"", "m_allowStop", lane.m_allowStop);
                writer.Write(" {0}=\"{1}\"", "m_useTerrainHeight", lane.m_useTerrainHeight);
                writer.Write(" {0}=\"{1:F}\"", "m_finalDirection", lane.m_finalDirection);
                writer.Write(" {0}=\"{1}\"", "m_similarLaneIndex", lane.m_similarLaneIndex);
                writer.Write(" {0}=\"{1}\"", "m_similarLaneCount", lane.m_similarLaneCount);
                writer.WriteLine(" />");
            }
            writer.WriteLine("</NetInfo>");
        }
        private class BezierPath : IComparable<BezierPath>
        {
            public float zlevel;
            public int zindex;
            public float zorder;
            public string name;
            public string cssclass;
            public Vector2 P0;
            public Vector2? P1;
            public Vector2? P2;
            public Vector2 P3;
            public BezierPath(float zlevel, int zindex, float zorder, string name, string cssclass, Vector2 p0, Vector2? p1, Vector2? p2, Vector2 p3)
            {
                this.zlevel = zlevel;
                this.zindex = zindex;
                this.zorder = zorder;
                this.name = name;
                this.cssclass = cssclass;
                this.P0 = p0;
                this.P1 = p1;
                this.P2 = p2;
                this.P3 = p3;
            }
            public int CompareTo(BezierPath other)
            {
                if (this.zlevel < other.zlevel) return -1;
                if (this.zlevel > other.zlevel) return 1;
                if (this.zindex < other.zindex) return -1;
                if (this.zindex > other.zindex) return 1;
                if (this.zorder < other.zorder) return -1;
                if (this.zorder > other.zorder) return 1;
                return this.name.CompareTo(other.name);
            }
        }
        static void BezierBisect(Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            out Vector2 ab, out Vector2 abbc, out Vector2 abbcbccd, out Vector2 bccd, out Vector2 cd)
        {
            ab = (a + b) / 2;
            Vector2 bc = (b + c) / 2;
            cd = (c + d) / 2;
            abbc = (ab + bc) / 2;
            bccd = (bc + cd) / 2;
            abbcbccd = (abbc + bccd) / 2;
        }
        private void clickDumpSVG(UIComponent component, UIMouseEventParameter eventParam)
        {
            using (StreamWriter writer = new StreamWriter("/Users/graydon/citymap.svg"))
            {
                NetManager nm = Singleton<NetManager>.instance;
                writer.Write(
                        "<?xml version=\"1.0\" standalone=\"no\"?>\n" +
                        "<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\" >\n" +
//                        "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"1080\" height=\"1080\" viewBox=\"-8640 -8640 17280 17280\">\n" +
                        "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"1440\" height=\"960\" viewBox=\"-1920 -1920 5760 3840\">\n" +
                        "<style language=\"text/css\"> /* <![CDATA[ */\n" +
                        "path {\n" +
                        "    fill: none;\n" +
                        "    stroke: #AAA;\n" +
                        "    stroke-linecap: round;\n" +
                        "    stroke-width: 12;\n" +
                        "    position: absolute;\n" +
                        "}\n" +
                        ".Line,\n" +
                        ".Connection,\n" +
                        ".Path,\n" +
                        ".Pipe\n" +
                        "{\n" +
                        "    display: none;\n" +
                        "}\n" +
                        ".Pedestrian\n" +
                        "{\n" +
//                        "    stroke-width: 6;\n" +
                        "}\n" +
                        ".Gravel\n" +
                        "{\n" +
                        "    /*stroke: #FA5;*/\n" +
                        "}\n" +
                        ".Gravel.outline\n" +
                        "{\n" +
                        "    display: none;\n" +
                        "}\n" +
                        ".Dam,\n" +
                        ".Basic,\n" +
                        ".Oneway.Road\n" +
                        "{\n" +
                        "    stroke: #AA5;\n" +
                        "}\n" +
                        ".Highway, .Medium\n" +
                        "{\n" +
//                        "    stroke-width: 18;\n" +
                        "}\n" +
                        ".Large\n" +
                        "{\n" +
                        "    stroke: #9C9;\n" +
                        "}\n" +
                        ".Medium\n" +
                        "{\n" +
                        "    stroke: #F99;\n" +
                        "}\n" +
                        ".Highway,\n" +
                        ".HighwayRamp\n" +
                        "{\n" +
                        "    stroke: #9CF;\n" +
                        "}\n" +
                        ".Highway.outline,\n" +
                        ".HighwayRamp.outline\n" +
                        "{\n" +
                        "    stroke: #333;\n" +
                        "}\n" +
                        ".Slope, .Tunnel\n" +
                        "{\n" +
                        "    /*opacity: 0.5;*/\n" +
                        "}\n" +
                        ".link\n" +
                        "{\n" +
                        "    stroke-linecap: butt;\n" +
                        "}\n" +
                        ".outline\n" +
                        "{\n" +
                        "    stroke: #666;\n" +
                        "}\n" +
                        ".Elevated.outline,\n" +
                        ".Bridge.outline,\n" +
                        ".Dam.outline,\n" +
                        ":not(g) > .Slope.outline\n" +
                        "{\n" +
                        "    stroke: #000;\n" +
                        "}\n" +
                        "");
                Dictionary<NetInfo, bool> dict = new Dictionary<NetInfo, bool>();
                for (int i = 0; i < NetManager.MAX_SEGMENT_COUNT; ++i)
                {
                    NetSegment ns = nm.m_segments.m_buffer[i];
                    if ((ns.m_flags & NetSegment.Flags.Created) != 0)
                    {
                        if (!dict.ContainsKey(ns.Info))
                        {
                            if (ns.Info.name.Contains("Line") ||
                                ns.Info.name.Contains("Connection") ||
                                ns.Info.name.Contains("Path") ||
                                ns.Info.name.Contains("Pipe"))
                            {
                                dict[ns.Info] = false;
                            }
                            else
                            {
                                dict[ns.Info] = true;
                            }
                            writer.WriteLine("{0} {1} stroke-width: {2}; {3}", (" " + ns.Info.name).Replace(" ", "."), '{', ns.Info.m_halfWidth * 2 - 4, '}');
                            writer.WriteLine("{0}.outline {1} stroke-width: {2}; {3}", (" " + ns.Info.name).Replace(" ", "."), '{', ns.Info.m_halfWidth * 2, '}');
                        }
                    }
                }
                writer.WriteLine("/* ]]> */ </style>");
                writer.WriteLine("<!--");
                foreach (NetInfo ni in dict.Keys) {
                    dumpNetInfo(ni, writer);
                }
                writer.WriteLine("-->");
                List<BezierPath> polys = new List<BezierPath>();
                //Dictionary<NetInfo, bool> dict = new Dictionary<NetInfo, bool>();
                for (int i = 0; i < NetManager.MAX_SEGMENT_COUNT; ++i)
                {
                    NetSegment ns = nm.m_segments.m_buffer[i];
                    if ((ns.m_flags & NetSegment.Flags.Created) != 0)
                    {
                        if (!dict.ContainsKey(ns.Info))
                        {
                            if (ns.Info.name.Contains("Line") ||
                                ns.Info.name.Contains("Connection") ||
                                ns.Info.name.Contains("Path") ||
                                ns.Info.name.Contains("Pipe"))
                            {
                                dict[ns.Info] = false;
                            }
                            else
                            {
                                dict[ns.Info] = true;
                            }
                        }
                        bool comment = !dict[ns.Info] ||
                            outOfBounds(nm.m_nodes.m_buffer[ns.m_startNode].m_position, nm.m_nodes.m_buffer[ns.m_endNode].m_position);
                        Vector3 middlePos1;
                        Vector3 middlePos2;
                        NetSegment.CalculateMiddlePoints(nm.m_nodes.m_buffer[ns.m_startNode].m_position,
                                ns.m_startDirection, nm.m_nodes.m_buffer[ns.m_endNode].m_position,
                                ns.m_endDirection, true, true, out middlePos1, out middlePos2);
                        if (comment)
                        {
                            writer.Write("<!--");
                            writer.Write("<path id=\"seg0x{0:X4}\" d=\"M", i);
                            writeCoord(nm.m_nodes.m_buffer[ns.m_startNode].m_position, writer);
                            writer.Write(" C");
                            writeCoord(middlePos1, writer);
                            writeCoord(middlePos2, writer);
                            writeCoord(nm.m_nodes.m_buffer[ns.m_endNode].m_position, writer);
                            writer.Write("\" class=\"{0}\" />", ns.Info.name);
                            writer.WriteLine("-->");
                        }
                        else if (NetSegment.IsStraight(nm.m_nodes.m_buffer[ns.m_startNode].m_position, ns.m_startDirection, nm.m_nodes.m_buffer[ns.m_endNode].m_position, ns.m_endDirection))
                        {
                            Vector2 oa = new Vector2(nm.m_nodes.m_buffer[ns.m_startNode].m_position.x, -nm.m_nodes.m_buffer[ns.m_startNode].m_position.z);
                            Vector2 od = new Vector2(nm.m_nodes.m_buffer[ns.m_endNode].m_position.x, -nm.m_nodes.m_buffer[ns.m_endNode].m_position.z);
                            Vector2 oad = (oa + od) / 2;
                            float startZ = nm.m_nodes.m_buffer[ns.m_startNode].m_position.y;
                            float endZ = nm.m_nodes.m_buffer[ns.m_endNode].m_position.y;
                            if (startZ < endZ)
                            {
                                polys.Add(new BezierPath(startZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-3-0", i), ns.Info.name + " outline", oa, null, null, oad));
                                polys.Add(new BezierPath(startZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-3-1", i), ns.Info.name, oa, null, null, oad));
                                Vector2 oaddd = (oad + od) / 2;
                                polys.Add(new BezierPath(endZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-8-2", i), ns.Info.name + " outline", oaddd, null, null, od));
                                polys.Add(new BezierPath(endZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-4-2", i), ns.Info.name + " outline link", oad, null, null, oaddd));
                                polys.Add(new BezierPath(endZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-C-3", i), ns.Info.name, oad, null, null, od));
                            }
                            else
                            {
                                polys.Add(new BezierPath(endZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-C-0", i), ns.Info.name + " outline", oad, null, null, od));
                                polys.Add(new BezierPath(endZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-C-1", i), ns.Info.name, oad, null, null, od));
                                Vector2 oaaad = (oa + oad) / 2;
                                polys.Add(new BezierPath(startZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-1-2", i), ns.Info.name + " outline", oa, null, null, oaaad));
                                polys.Add(new BezierPath(startZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-2-2", i), ns.Info.name + " outline link", oaaad, null, null, oad));
                                polys.Add(new BezierPath(startZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-3-3", i), ns.Info.name, oa, null, null, oad));
                            }
                        }
                        else
                        {
                            Vector2 oa = new Vector2(nm.m_nodes.m_buffer[ns.m_startNode].m_position.x, -nm.m_nodes.m_buffer[ns.m_startNode].m_position.z);
                            Vector2 ob = new Vector2(middlePos1.x, -middlePos1.z);
                            Vector2 oc = new Vector2(middlePos2.x, -middlePos2.z);
                            Vector2 od = new Vector2(nm.m_nodes.m_buffer[ns.m_endNode].m_position.x, -nm.m_nodes.m_buffer[ns.m_endNode].m_position.z);
                            Vector2 oab, oabbc, oabbcbccd, obccd, ocd;
                            BezierBisect(oa, ob, oc, od, out oab, out oabbc, out oabbcbccd, out obccd, out ocd);
                            float startZ = nm.m_nodes.m_buffer[ns.m_startNode].m_position.y;
                            float endZ = nm.m_nodes.m_buffer[ns.m_endNode].m_position.y;
                            if (startZ < endZ)
                            {
                                polys.Add(new BezierPath(startZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-3-0", i), ns.Info.name + " outline", oa, oab, oabbc, oabbcbccd));
                                polys.Add(new BezierPath(startZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-3-1", i), ns.Info.name, oa, oab, oabbc, oabbcbccd));
                                Vector2 xab, xabbc, xabbcbccd, xbccd, xcd;
                                BezierBisect(oabbcbccd, obccd, ocd, od, out xab, out xabbc, out xabbcbccd, out xbccd, out xcd);
                                polys.Add(new BezierPath(endZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-8-2", i), ns.Info.name + " outline", xabbcbccd, xbccd, xcd, od));
                                polys.Add(new BezierPath(endZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-4-2", i), ns.Info.name + " outline link", oabbcbccd, xab, xabbc, xabbcbccd));
                                polys.Add(new BezierPath(endZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-C-3", i), ns.Info.name, oabbcbccd, obccd, ocd, od));
                            }
                            else
                            {
                                polys.Add(new BezierPath(endZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-C-0", i), ns.Info.name + " outline", oabbcbccd, obccd, ocd, od));
                                polys.Add(new BezierPath(endZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-C-1", i), ns.Info.name, oabbcbccd, obccd, ocd, od));
                                Vector2 xab, xabbc, xabbcbccd, xbccd, xcd;
                                BezierBisect(oa, oab, oabbc, oabbcbccd, out xab, out xabbc, out xabbcbccd, out xbccd, out xcd);
                                polys.Add(new BezierPath(startZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-1-2", i), ns.Info.name + " outline", oa, xab, xabbc, xabbcbccd));
                                polys.Add(new BezierPath(startZ, 0, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-2-2", i), ns.Info.name + " outline link", xabbcbccd, xbccd, xcd, oabbcbccd));
                                polys.Add(new BezierPath(startZ, 1, ns.Info.m_halfWidth, String.Format("seg-0x{0:X4}-3-3", i), ns.Info.name, oa, oab, oabbc, oabbcbccd));
                            }
                        }
                    }
                }
                polys.Sort();
                foreach (BezierPath e in polys)
                {
                    writer.Write("<path id=\"{0}\" d=\"", e.name);
                    writePath(writer, e.P0, e.P1, e.P2, e.P3);
                    writer.WriteLine("\" class=\"{0}\" />", e.cssclass);
                }
                writer.Write("</svg>\n");
            }
        }

        public override void Update()
        {
            switch (_uistate)
            {
                case UIState.None: _basePanel(); break;
                case UIState.SwitchTrafficLight: _switchTrafficPanel(); break;
                case UIState.AddStopSign: _addStopSignPanel(); break;
                case UIState.ManualSwitch: _manualSwitchPanel(); break;
                case UIState.TimedControlNodes: _timedControlNodesPanel(); break;
                case UIState.TimedControlLights: _timedControlLightsPanel(); break;
                case UIState.LaneChange: _laneChangePanel(); break;
                case UIState.Crosswalk: _crosswalkPanel(); break;
            }
        }

        private void _basePanel()
        {

        }

        private void _switchTrafficPanel()
        {

        }

        private void _addStopSignPanel()
        {

        }

        private void _manualSwitchPanel()
        {

        }

        private void _timedControlNodesPanel()
        {
        }

        private void _timedControlLightsPanel()
        {
        }

        private void _laneChangePanel()
        {
            if (TrafficLightTool.SelectedSegment != 0)
            {
                NetManager instance = Singleton<NetManager>.instance;

                var segment = instance.m_segments.m_buffer[TrafficLightTool.SelectedSegment];

                var info = segment.Info;

                uint num2 = segment.m_lanes;
                int num3 = 0;

                int offsetIdx = 0;

                NetInfo.Direction dir = NetInfo.Direction.Forward;
                if (segment.m_startNode == TrafficLightTool.SelectedNode)
                    dir = NetInfo.Direction.Backward;
                var dir3 = ((segment.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None) ? dir : NetInfo.InvertDirection(dir);

                while (num3 < info.m_lanes.Length && num2 != 0u)
                {
                    if (info.m_lanes[num3].m_laneType != NetInfo.LaneType.Pedestrian && info.m_lanes[num3].m_direction == dir3)
                    {
                        //segmentLights[num3].Show();
                        //segmentLights[num3].relativePosition = new Vector3(35f, (float)(xPos + (offsetIdx * 40f)));
                        //segmentLights[num3].text = ((NetLane.Flags)instance.m_lanes.m_buffer[num2].m_flags & ~NetLane.Flags.Created).ToString();

                        //if (segmentLights[num3].containsMouse)
                        //{
                        //    if (Input.GetMouseButton(0) && !segmentMouseDown)
                        //    {
                        //        switchLane(num2);
                        //        segmentMouseDown = true;

                        //        if (
                        //            !TrafficPriority.isPrioritySegment(TrafficLightTool.SelectedNode,
                        //                TrafficLightTool.SelectedSegment))
                        //        {
                        //            TrafficPriority.addPrioritySegment(TrafficLightTool.SelectedNode, TrafficLightTool.SelectedSegment, PrioritySegment.PriorityType.None);
                        //        }
                        //    }
                        //}

                        offsetIdx++;
                    }

                    num2 = instance.m_lanes.m_buffer[(int)((UIntPtr)num2)].m_nextLane;
                    num3++;
                }
            }
        }

        public void switchLane(uint laneID)
        {
            var flags = (NetLane.Flags)Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags;

            if ((flags & NetLane.Flags.LeftForwardRight) == NetLane.Flags.LeftForwardRight)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.LeftForwardRight) | NetLane.Flags.Forward);
            }
            else if ((flags & NetLane.Flags.ForwardRight) == NetLane.Flags.ForwardRight)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.ForwardRight) | NetLane.Flags.LeftForwardRight);
            }
            else if ((flags & NetLane.Flags.LeftRight) == NetLane.Flags.LeftRight)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.LeftRight) | NetLane.Flags.ForwardRight);
            }
            else if ((flags & NetLane.Flags.LeftForward) == NetLane.Flags.LeftForward)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.LeftForward) | NetLane.Flags.LeftRight);
            }
            else if ((flags & NetLane.Flags.Right) == NetLane.Flags.Right)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.Right) | NetLane.Flags.LeftForward);
            }
            else if ((flags & NetLane.Flags.Left) == NetLane.Flags.Left)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.Left) | NetLane.Flags.Right);
            }
            else if ((flags & NetLane.Flags.Forward) == NetLane.Flags.Forward)
            {
                Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_flags =
                    (ushort) ((flags & ~NetLane.Flags.Forward) | NetLane.Flags.Left);
            }
        }

        private void _crosswalkPanel()
        {
        }
    }
}
