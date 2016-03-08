using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
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
        private static UIButton buttonDumpNM;

        public static TrafficLightTool trafficLightTool;

        public override void Start()
        {
            inited = true;

            trafficLightTool = LoadingExtension.Instance.TrafficLightTool;

            this.backgroundSprite = "GenericPanel";
            this.color = new Color32(75, 75, 135, 255);
            this.width = 240;
//            this.height = !LoadingExtension.PathfinderIncompatibility ? 390 : 310;
            this.height = !LoadingExtension.PathfinderIncompatibility ? 430 : 350;
            this.relativePosition = new Vector3(80f, 50f);

            UILabel title = this.AddUIComponent<UILabel>();
            title.text = "Traffic Manager";
            title.width = 200;
            title.textAlignment = UIHorizontalAlignment.Center;
            title.relativePosition = new Vector3(20f, 5f);

            if (!LoadingExtension.PathfinderIncompatibility)
            {
                buttonSwitchTraffic = _createButton("Switch traffic lights", new Vector3(20f, 30f), clickSwitchTraffic);
                buttonPrioritySigns = _createButton("Add priority signs", new Vector3(20f, 70f), clickAddPrioritySigns);
                buttonManualControl = _createButton("Manual traffic lights", new Vector3(20f, 110f), clickManualControl);
                buttonTimedMain = _createButton("Timed traffic lights", new Vector3(20f, 150f), clickTimedAdd);
                buttonLaneChange = _createButton("Change lanes", new Vector3(20f, 190f), clickChangeLanes);
                //buttonLaneRestrictions = _createButton("Road Restrictions", new Vector3(20f, 230f), clickLaneRestrictions);
                buttonCrosswalk = _createButton("Add/Remove Crosswalk", new Vector3(20f, 230f), clickCrosswalk);
                buttonClearTraffic = _createButton("Clear Traffic", new Vector3(20f, 270f), clickClearTraffic);
                buttonToggleDespawn = _createButton(LoadingExtension.Instance.despawnEnabled ? "Disable despawning" : "Enable despawning", new Vector3(20f, 310f), clickToggleDespawn);
                buttonDumpSVG = _createButton("Export to SVG", new Vector3(20f, 350f), clickDumpSVG);
                buttonDumpNM = _createButton("Dump NetManager", new Vector3(20f, 390f), clickDumpNM);

            }
            else
            {
                buttonSwitchTraffic = _createButton("Switch traffic lights", new Vector3(20f, 30f), clickSwitchTraffic);
                buttonPrioritySigns = _createButton("Add priority signs", new Vector3(20f, 70f), clickAddPrioritySigns);
                buttonManualControl = _createButton("Manual traffic lights", new Vector3(20f, 110f), clickManualControl);
                buttonTimedMain = _createButton("Timed traffic lights", new Vector3(20f, 150f), clickTimedAdd);
                buttonCrosswalk = _createButton("Add/Remove Crosswalk", new Vector3(20f, 190f), clickCrosswalk);
                buttonClearTraffic = _createButton("Clear Traffic", new Vector3(20f, 230f), clickClearTraffic);
                buttonDumpSVG = _createButton("Export to SVG", new Vector3(20f, 270f), clickDumpSVG);
                buttonDumpNM = _createButton("Dump NetManager", new Vector3(20f, 310f), clickDumpNM);
            }
        }

        private UIButton _createButton(string text, Vector3 pos, MouseEventHandler eventClick)
        {
            var button = this.AddUIComponent<UIButton>();
            button.width = 200;
            button.height = 30;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = Color.white;
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
            writer.Write(" {0,8:F2} {1,8:F2}", coord.x, -coord.z);
        }
        private void writeCoord(Vector2 coord, StreamWriter writer)
        {
            writer.Write(" {0,8:F2} {1,8:F2}", coord.x, coord.y);
        }
        private void writePath(StreamWriter writer, Vector2 p0, Vector2? p1, Vector2? p2, Vector2? p3)
        {
            writer.Write("M");
            writeCoord(p0, writer);
            if (p3 == null)
            {
                writer.Write(" Z");
                return;
            }
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
            writeCoord(p3.Value, writer);
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
//                writer.Write(" {0}=\"{1}\"", "m_allowStop", lane.m_allowStop);
                writer.Write(" {0}=\"{1}\"", "m_useTerrainHeight", lane.m_useTerrainHeight);
                writer.Write(" {0}=\"{1:F}\"", "m_finalDirection", lane.m_finalDirection);
                writer.Write(" {0}=\"{1}\"", "m_similarLaneIndex", lane.m_similarLaneIndex);
                writer.Write(" {0}=\"{1}\"", "m_similarLaneCount", lane.m_similarLaneCount);
                if (lane.m_laneProps != null)
                {
                    writer.WriteLine(">");
                    foreach (NetLaneProps.Prop prop in lane.m_laneProps.m_props)
                    {
                        writer.Write("\t\t<Prop");
                        writer.Write(" {0}=\"{1:F}\"", "m_flagsRequired", prop.m_flagsRequired);
                        writer.Write(" {0}=\"{1:F}\"", "m_flagsForbidden", prop.m_flagsForbidden);
                        writer.Write(" {0}=\"{1:F}\"", "m_startFlagsRequired", prop.m_startFlagsRequired);
                        writer.Write(" {0}=\"{1:F}\"", "m_startFlagsForbidden", prop.m_startFlagsForbidden);
                        writer.Write(" {0}=\"{1:F}\"", "m_endFlagsRequired", prop.m_endFlagsRequired);
                        writer.Write(" {0}=\"{1:F}\"", "m_endFlagsForbidden", prop.m_endFlagsForbidden);
                        writer.Write(" {0}=\"{1}\"", "m_colorMode", prop.m_colorMode);
                        writer.Write(" {0}=\"{1}\"", "m_prop", prop.m_prop);
                        writer.Write(" {0}=\"{1}\"", "m_tree", prop.m_tree);
                        writer.Write(" {0}=\"{1}\"", "m_position", prop.m_position);
                        writer.Write(" {0}=\"{1}\"", "m_angle", prop.m_angle);
                        writer.Write(" {0}=\"{1}\"", "m_segmentOffset", prop.m_segmentOffset);
                        writer.Write(" {0}=\"{1}\"", "m_repeatDistance", prop.m_repeatDistance);
                        writer.Write(" {0}=\"{1}\"", "m_minLength", prop.m_minLength);
                        writer.Write(" {0}=\"{1}\"", "m_cornerAngle", prop.m_cornerAngle);
                        writer.Write(" {0}=\"{1}\"", "m_probability", prop.m_probability);
                        writer.Write(" {0}=\"{1}\"", "m_finalProp", prop.m_finalProp);
                        writer.Write(" {0}=\"{1}\"", "m_finalTree", prop.m_finalTree);
                        writer.WriteLine(" />");
                    }
                    writer.WriteLine("\t</Lane>");
                }
                else
                {
                    writer.WriteLine(" />");
                }
            }
            writer.WriteLine("</NetInfo>");
        }
        private class PathSegment : IComparable<PathSegment>
        {
            public float zlevel;
            public int zindex;
            public float zorder;
            public string name;
            public string cssclass;
            public Vector2 P0;
            public Vector2? P1;
            public Vector2? P2;
            public Vector2? P3;
            public PathSegment(float zlevel, int zindex, float zorder, string name, string cssclass, Vector2 p0, Vector2? p1, Vector2? p2, Vector2? p3)
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
            public int CompareTo(PathSegment other)
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
                        "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"1440\" height=\"1440\" viewBox=\"-1920 -1920 5760 5760\">\n" +
                        "<rect id=\"map\" x=\"-8640\" y=\"-8640\" width=\"17280\" height=\"17280\" style=\"stroke: none; fill: #EFD;\" />\n" +
                        "<rect id=\"city\" x=\"-960\" y=\"-960\" width=\"3840\" height=\"3840\" style=\"stroke: none; fill: #EEE;\" />\n" +
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
                        ".Pipe,\n" +
                        ".Pedestrian.outline,\n" +
                        ".Train.outline\n" +
                        "{\n" +
                        "    display: none;\n" +
                        "}\n" +
                        ".Gravel.Road\n" +
                        "{\n" +
                        "    stroke: #FFF;\n" +
                        "}\n" +
                        ".Dam,\n" +
                        ".Basic,\n" +
                        ".Oneway.Road:not(.outline)\n" +
                        "{\n" +
                        "    stroke: #FF9;\n" +
                        "}\n" +
                        ".Medium\n" +
                        "{\n" +
                        "    stroke: #FC9;\n" +
                        "    stroke-width: 15;\n" +
                        "}\n" +
                        ".Medium.outline\n" +
                        "{\n" +
                        "    stroke-width: 17;\n" +
                        "}\n" +
                        ".Large\n" +
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
                        ".Pedestrian\n" +
                        "{\n" +
                        "    stroke: #666;\n" +
                        "    stroke-width: 2;\n" +
                        "}\n" +
                        ".Train\n" +
                        "{\n" +
                        "    stroke: #363;\n" +
                        "    stroke-width: 4;\n" +
                        "}\n" +
                        "");
                Dictionary<NetInfo, uint> dict = new Dictionary<NetInfo, uint>();
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
                                dict[ns.Info] = 0;
                                continue;
                            }
                            else if (ns.Info.name.StartsWith("Pedestrian")) { dict[ns.Info] = 1; }
                            else if (ns.Info.name.StartsWith("HighwayRamp")) { dict[ns.Info] = 2; }
                            else if (ns.Info.name.StartsWith("Gravel")) { dict[ns.Info] = 3; }
                            else if (ns.Info.name.StartsWith("Basic") || ns.Info.name.StartsWith("Oneway") || ns.Info.name.StartsWith("Dam")) { dict[ns.Info] = 4; }
                            else if (ns.Info.name.StartsWith("Medium")) { dict[ns.Info] = 5; }
                            else if (ns.Info.name.StartsWith("Large")) { dict[ns.Info] = 6; }
                            else if (ns.Info.name.StartsWith("Highway")) { dict[ns.Info] = 7; }
                            else if (ns.Info.name.StartsWith("Train")) { dict[ns.Info] = 8; }
                            else
                            {
                                dict[ns.Info] = 0;
                                writer.WriteLine("/* {0} ??? */", ns.Info.name);
                                continue;
                            }
                            if (ns.Info.name.EndsWith("Tunnel") || ns.Info.name.EndsWith("Slope") || ns.Info.name.EndsWith("Elevated") || ns.Info.name.EndsWith("Bridge"))
                            {
                                continue;
                            }
                            float sumwidth = 0;
                            float maxwidth = 0;
                            foreach (NetInfo.Lane lane in ns.Info.m_lanes)
                            {
                                if (lane.m_laneType == NetInfo.LaneType.Vehicle && lane.m_vehicleType == VehicleInfo.VehicleType.Car)
                                {
                                    sumwidth += lane.m_width;
                                    maxwidth = Math.Max(maxwidth, 2 * Math.Abs(lane.m_position) + lane.m_width);
                                }
                            }
                            float effwidth = (sumwidth + maxwidth) / 2f;
                            if (effwidth > 0)
                            {
                                string selector = (" " + ns.Info.name).Replace(" ", ".");
                                writer.WriteLine("{0} {1} stroke-width: {2}; {3}", selector, '{', effwidth, '}');
                                writer.WriteLine("{0}.outline {1} stroke-width: {2}; {3}", selector, '{', effwidth + 2, '}');
                            }
                        }
                    }
                }
                writer.WriteLine("/* ]]> */ </style>");
                writer.WriteLine("<!--");
                foreach (NetInfo ni in dict.Keys) {
                    dumpNetInfo(ni, writer);
                }
                writer.WriteLine("-->");
                List<PathSegment> polys = new List<PathSegment>();
                List<PathSegment> underground = new List<PathSegment>();
                List<PathSegment> medians = new List<PathSegment>();
                for (int i = 0; i < NetManager.MAX_SEGMENT_COUNT; ++i)
                {
                    NetSegment ns = nm.m_segments.m_buffer[i];
                    if ((ns.m_flags & NetSegment.Flags.Created) != 0)
                    {
                        uint order = dict[ns.Info];
                        bool comment = order == 0 ||
                            outOfBounds(nm.m_nodes.m_buffer[ns.m_startNode].m_position, nm.m_nodes.m_buffer[ns.m_endNode].m_position);
                        float startZ = nm.m_nodes.m_buffer[ns.m_startNode].m_position.y;
                        float endZ = nm.m_nodes.m_buffer[ns.m_endNode].m_position.y;
                        Vector3 middlePos1;
                        Vector3 middlePos2;
                        NetSegment.CalculateMiddlePoints(nm.m_nodes.m_buffer[ns.m_startNode].m_position,
                                ns.m_startDirection, nm.m_nodes.m_buffer[ns.m_endNode].m_position,
                                ns.m_endDirection, true, true, out middlePos1, out middlePos2);
                        string cssclass = ns.Info.name == "HighwayRampElevated" ? "HighwayRamp Elevated" : ns.Info.name;
                        bool hasMedian = cssclass.StartsWith("Medium ");
                        List<PathSegment> startL = polys;
                        List<PathSegment> endL = polys;
                        if (cssclass.EndsWith(" Tunnel"))
                        {
                            startL = underground;
                            endL = underground;
                        }
                        else if (cssclass.EndsWith(" Slope"))
                        {
                            if (startZ < endZ)
                            {
                                startL = underground;
                            }
                            else
                            {
                                endL = underground;
                            }
                        }
                        if (comment)
                        {
                            /*
                            writer.Write("<!--");
                            writer.Write("<path id=\"seg0x{0:X4}\" d=\"M", i);
                            writeCoord(nm.m_nodes.m_buffer[ns.m_startNode].m_position, writer);
                            writer.Write(" C");
                            writeCoord(middlePos1, writer);
                            writeCoord(middlePos2, writer);
                            writeCoord(nm.m_nodes.m_buffer[ns.m_endNode].m_position, writer);
                            writer.Write("\" class=\"{0}\" />", cssclass);
                            writer.WriteLine("-->");
                            */
                        }
                        else if (NetSegment.IsStraight(nm.m_nodes.m_buffer[ns.m_startNode].m_position, ns.m_startDirection, nm.m_nodes.m_buffer[ns.m_endNode].m_position, ns.m_endDirection))
                        {
                            Vector2 oa = new Vector2(nm.m_nodes.m_buffer[ns.m_startNode].m_position.x, -nm.m_nodes.m_buffer[ns.m_startNode].m_position.z);
                            Vector2 od = new Vector2(nm.m_nodes.m_buffer[ns.m_endNode].m_position.x, -nm.m_nodes.m_buffer[ns.m_endNode].m_position.z);
                            Vector2 oad = (oa + od) / 2;

                            if (startZ < endZ)
                            {
                                startL.Add(new PathSegment(startZ, 0, order, String.Format("seg-0x{0:X4}-3-0", i), cssclass + " outline", oa, null, null, oad));
                                startL.Add(new PathSegment(startZ, 1, order, String.Format("seg-0x{0:X4}-3-1", i), cssclass, oa, null, null, oad));
                                endL.Add(new PathSegment(endZ, 0, order, String.Format("seg-0x{0:X4}-8-2", i), cssclass + " outline", od, null, null, null));
                                endL.Add(new PathSegment(endZ, 0, order, String.Format("seg-0x{0:X4}-C-2", i), cssclass + " outline link", oad, null, null, od));
                                endL.Add(new PathSegment(endZ, 1, order, String.Format("seg-0x{0:X4}-C-3", i), cssclass, oad, null, null, od));
                            }
                            else
                            {
                                endL.Add(new PathSegment(endZ, 0, order, String.Format("seg-0x{0:X4}-C-0", i), cssclass + " outline", oad, null, null, od));
                                endL.Add(new PathSegment(endZ, 1, order, String.Format("seg-0x{0:X4}-C-1", i), cssclass, oad, null, null, od));
                                startL.Add(new PathSegment(startZ, 0, order, String.Format("seg-0x{0:X4}-1-2", i), cssclass + " outline", oa, null, null, null));
                                startL.Add(new PathSegment(startZ, 0, order, String.Format("seg-0x{0:X4}-3-2", i), cssclass + " outline link", oa, null, null, oad));
                                startL.Add(new PathSegment(startZ, 1, order, String.Format("seg-0x{0:X4}-3-3", i), cssclass, oa, null, null, oad));
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
                            if (startZ < endZ)
                            {
                                startL.Add(new PathSegment(startZ, 0, order, String.Format("seg-0x{0:X4}-3-0", i), cssclass + " outline", oa, oab, oabbc, oabbcbccd));
                                startL.Add(new PathSegment(startZ, 1, order, String.Format("seg-0x{0:X4}-3-1", i), cssclass, oa, oab, oabbc, oabbcbccd));
                                endL.Add(new PathSegment(endZ, 0, order, String.Format("seg-0x{0:X4}-8-2", i), cssclass + " outline", od, null, null, null));
                                endL.Add(new PathSegment(endZ, 0, order, String.Format("seg-0x{0:X4}-C-2", i), cssclass + " outline link", oabbcbccd, obccd, ocd, od));
                                endL.Add(new PathSegment(endZ, 1, order, String.Format("seg-0x{0:X4}-C-3", i), cssclass, oabbcbccd, obccd, ocd, od));
                            }
                            else
                            {
                                endL.Add(new PathSegment(endZ, 0, order, String.Format("seg-0x{0:X4}-C-0", i), cssclass + " outline", oabbcbccd, obccd, ocd, od));
                                endL.Add(new PathSegment(endZ, 1, order, String.Format("seg-0x{0:X4}-C-1", i), cssclass, oabbcbccd, obccd, ocd, od));
                                startL.Add(new PathSegment(startZ, 0, order, String.Format("seg-0x{0:X4}-1-2", i), cssclass + " outline", oa, null, null, null));
                                startL.Add(new PathSegment(startZ, 0, order, String.Format("seg-0x{0:X4}-3-2", i), cssclass + " outline link", oa, oab, oabbc, oabbcbccd));
                                startL.Add(new PathSegment(startZ, 1, order, String.Format("seg-0x{0:X4}-3-3", i), cssclass, oa, oab, oabbc, oabbcbccd));
                            }
                        }
                    }
                }
                underground.Sort();
                writer.WriteLine("<g id=\"underground\" style=\"opacity: 0.5;\">");
                foreach (PathSegment e in underground)
                {
                    writer.Write("<path id=\"{0}\" d=\"", e.name);
                    writePath(writer, e.P0, e.P1, e.P2, e.P3);
                    writer.WriteLine("\" class=\"{0}\" />", e.cssclass);
                }
                writer.WriteLine("</g>");
                polys.Sort();
                foreach (PathSegment e in polys)
                {
                    writer.Write("<path id=\"{0}\" d=\"", e.name);
                    writePath(writer, e.P0, e.P1, e.P2, e.P3);
                    writer.WriteLine("\" class=\"{0}\" />", e.cssclass);
                }
                writer.Write("</svg>\n");
            }
        }
        private void clickDumpNM(UIComponent component, UIMouseEventParameter eventParam)
        {
            NetManager instance = Singleton<NetManager>.instance;
            uint frame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            using (StreamWriter writer = new StreamWriter("/Users/graydon/NetManager." + frame + ".m_nodes.dat"))
            {
                Array16<NetNode> nodes = instance.m_nodes;
                uint unusedCount = (uint) nodes.GetType().GetField("m_unusedCount", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(nodes);
                ushort[] unusedItems = (ushort[]) nodes.GetType().GetField("m_unusedItems", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(nodes);
                bool[] missing = new bool[nodes.m_size];
                for (uint i = 0; i < unusedCount; ++i)
                {
                    missing[unusedItems[i]] = true;
                }
                for (int i = 0; i < nodes.m_buffer.Length; ++i)
                {
                    if (missing[i])
                    {
                        continue;
                    }
                    writer.WriteLine("{0}n\t{1}s\t{2}s\t{3}s\t{4}s\t{5}s\t{6}s\t{7}s\t{8}s\t{9}l\t{10}o\t{11}L\t{12}x\t{13}z", i,
                            nodes.m_buffer[i].m_segment0,
                            nodes.m_buffer[i].m_segment1,
                            nodes.m_buffer[i].m_segment2,
                            nodes.m_buffer[i].m_segment3,
                            nodes.m_buffer[i].m_segment4,
                            nodes.m_buffer[i].m_segment5,
                            nodes.m_buffer[i].m_segment6,
                            nodes.m_buffer[i].m_segment7,
                            nodes.m_buffer[i].m_nextLaneNode,
                            nodes.m_buffer[i].m_laneOffset,
                            nodes.m_buffer[i].m_lane,
                            nodes.m_buffer[i].m_position.x,
                            nodes.m_buffer[i].m_position.z);
                }
            }
            using (StreamWriter writer = new StreamWriter("/Users/graydon/NetManager." + frame + ".m_segments.dat"))
            {
                Array16<NetSegment> segments = instance.m_segments;
                uint unusedCount = (uint) segments.GetType().GetField("m_unusedCount", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(segments);
                ushort[] unusedItems = (ushort[]) segments.GetType().GetField("m_unusedItems", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(segments);
                bool[] missing = new bool[segments.m_size];
                for (uint i = 0; i < unusedCount; ++i)
                {
                    missing[unusedItems[i]] = true;
                }
                for (int i = 0; i < segments.m_buffer.Length; ++i)
                {
                    if (missing[i])
                    {
                        continue;
                    }
                    writer.Write("{0}", i);
                    //writer.Write("\tm_problems: {0}", segments.m_buffer[i].m_problems);
                    //writer.Write("\tm_bounds: {0}", segments.m_buffer[i].m_bounds);
                    //writer.Write("\tm_middlePosition: {0}", segments.m_buffer[i].m_middlePosition);
                    writer.Write("\tm_startDirection: {0}", segments.m_buffer[i].m_startDirection);
                    writer.Write("\tm_endDirection: {0}", segments.m_buffer[i].m_endDirection);
                    writer.Write("\tm_flags: {0}", segments.m_buffer[i].m_flags);
                    writer.Write("\tm_averageLength: {0}", segments.m_buffer[i].m_averageLength);
                    //writer.Write("\tm_buildIndex: {0}", segments.m_buffer[i].m_buildIndex);
                    //writer.Write("\tm_modifiedIndex: {0}", segments.m_buffer[i].m_modifiedIndex);
                    writer.Write("\tm_lanes: {0}", segments.m_buffer[i].m_lanes);
                    //writer.Write("\tm_path: {0}", segments.m_buffer[i].m_path);
                    writer.Write("\tm_startNode: {0}", segments.m_buffer[i].m_startNode);
                    writer.Write("\tm_endNode: {0}", segments.m_buffer[i].m_endNode);
                    //writer.Write("\tm_blockStartLeft: {0}", segments.m_buffer[i].m_blockStartLeft);
                    //writer.Write("\tm_blockStartRight: {0}", segments.m_buffer[i].m_blockStartRight);
                    //writer.Write("\tm_blockEndLeft: {0}", segments.m_buffer[i].m_blockEndLeft);
                    //writer.Write("\tm_blockEndRight: {0}", segments.m_buffer[i].m_blockEndRight);
                    writer.Write("\tm_trafficBuffer: {0}", segments.m_buffer[i].m_trafficBuffer);
                    writer.Write("\tm_startLeftSegment: {0}", segments.m_buffer[i].m_startLeftSegment);
                    writer.Write("\tm_startRightSegment: {0}", segments.m_buffer[i].m_startRightSegment);
                    writer.Write("\tm_endLeftSegment: {0}", segments.m_buffer[i].m_endLeftSegment);
                    writer.Write("\tm_endRightSegment: {0}", segments.m_buffer[i].m_endRightSegment);
                    //writer.Write("\tm_infoIndex: {0}", segments.m_buffer[i].m_infoIndex);
                    //writer.Write("\tm_nextGridSegment: {0}", segments.m_buffer[i].m_nextGridSegment);
                    writer.Write("\tm_trafficDensity: {0}", segments.m_buffer[i].m_trafficDensity);
                    writer.Write("\tm_trafficLightState0: {0}", segments.m_buffer[i].m_trafficLightState0);
                    writer.Write("\tm_trafficLightState1: {0}", segments.m_buffer[i].m_trafficLightState1);
                    writer.Write("\tm_cornerAngleStart: {0}", segments.m_buffer[i].m_cornerAngleStart);
                    writer.Write("\tm_cornerAngleEnd: {0}", segments.m_buffer[i].m_cornerAngleEnd);
                    //writer.Write("\tm_fireCoverage: {0}", segments.m_buffer[i].m_fireCoverage);
                    //writer.Write("\tm_wetness: {0}", segments.m_buffer[i].m_wetness);
                    writer.Write("\tm_condition: {0}", segments.m_buffer[i].m_condition);
                    writer.WriteLine();
                }
            }
            using (StreamWriter writer = new StreamWriter("/Users/graydon/NetManager." + frame + ".m_lanes.dat"))
            {
              try
              {
                Array32<NetLane> lanes = instance.m_lanes;
                uint unusedCount = (uint) lanes.GetType().GetField("m_unusedCount", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(lanes);
                uint[] unusedItems = (uint[]) lanes.GetType().GetField("m_unusedItems", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(lanes);
                bool[] missing = new bool[lanes.m_size];
                for (uint i = 0; i < unusedCount; ++i)
                {
                    missing[unusedItems[i]] = true;
                }
                for (int i = 0; i < lanes.m_buffer.Length; ++i)
                {
                    if (missing[i])
                    {
                        continue;
                    }
                    writer.Write("{0}", i);
                    writer.Write("\tm_bezier: {0}", lanes.m_buffer[i].m_bezier);
                    writer.Write("\tm_length: {0}", lanes.m_buffer[i].m_length);
                    writer.Write("\tm_curve: {0}", lanes.m_buffer[i].m_curve);
                    writer.Write("\tm_nextLane: {0}", lanes.m_buffer[i].m_nextLane);
                    writer.Write("\tm_flags: {0}", lanes.m_buffer[i].m_flags);
                    writer.Write("\tm_segment: {0}", lanes.m_buffer[i].m_segment);
                    writer.Write("\tm_nodes: {0}", lanes.m_buffer[i].m_nodes);
                    //writer.Write("\tm_lastReserveID: {0}", lanes.m_buffer[i].m_lastReserveID);
                    writer.Write("\tm_firstTarget: {0}", lanes.m_buffer[i].m_firstTarget);
                    writer.Write("\tm_lastTarget: {0}", lanes.m_buffer[i].m_lastTarget);
                    //writer.Write("\tm_reservedPrev: {0}", lanes.m_buffer[i].m_reservedPrev);
                    //writer.Write("\tm_reservedNext: {0}", lanes.m_buffer[i].m_reservedNext);
                    //writer.Write("\tm_reservedFrame: {0}", lanes.m_buffer[i].m_reservedFrame);
                    writer.WriteLine();
                }
              }
              catch (Exception ex)
              {
                  Log.Warning(ex.ToString());
              }
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

                    num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
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
