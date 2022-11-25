using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System;
using System.Reflection;
using System.Diagnostics;

namespace ElectricityDLL
{
    [JsonObject]
    public class JsonJunction
    {
        public TerminalKey TerminalKey { get; set; }
        public string OwnerName { get; set; }
        public JsonPointF Point { get; set; }
        public WireArea Area { get; set; }
        public bool IsMoved { get; set; }
    }

    [JsonObject]
    public class JsonPointF
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    [JsonObject]
    public class JsonWire
    {
        public string Name { get; set; }
        public List<JsonJunction> Junctions { get; set; }
        public List<JsonPointF> Points { get; set; }
    }
    [JsonObject]
    public abstract class JsonElement
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public FaultType Fault { get; set; }
        public ComponentType Type { get; set; }
    }

    [JsonObject]
    public class JsonAmmeter : JsonElement
    {
        public int Scale { get; set; }
    }

    [JsonObject]
    public class JsonVoltmeter : JsonElement
    {
        public int Scale { get; set; }
    }

    [JsonObject]
    public class JsonBatteryCase : JsonElement
    {
        public float Voltage { get; set; }
    }

    [JsonObject]
    public class JsonFan : JsonElement
    {
        public float RatedVoltage { get; set; }
        public float Resistance { get; set; }
        public float InductiveReactance { get; set; }
    }

    [JsonObject]
    public class JsonResistor : JsonElement
    {
        public float Resistance { get; set; }
    }

    [JsonObject]
    public class JsonLampstand : JsonElement
    {
        public float RatedVoltage { get; set; }
        public float RatedPower { get; set; }
    }

    [JsonObject]
    public class JsonRheostat : JsonElement
    {
        public float MaxResistance { get; set; }
        public PointF VanePoint { get; set; }
    }

    [JsonObject]
    public class JsonSwitch : JsonElement
    {
        public WorkStat Stat { get; set; }
    }

    [JsonObject]
    public class JsonWorkbench
    {
        [JsonProperty(Consts.Json_ShowElementName)]
        public bool IsShowElementName { get; set; }
        [JsonProperty(Consts.Json_ShowWireName)]
        public bool IsShowWireName { get; set; }
        [JsonProperty(Consts.Json_Locked)]
        public bool IsLocked { get; set; }
        [JsonProperty(Consts.Json_AutoAnalyze)]
        public bool IsAutoAnalyze { get; set; }
        [JsonProperty(Consts.Json_Elements)]
        public List<JsonElement> Elements { get; set; } = new List<JsonElement>();
        [JsonProperty("BCS")]
        public List<JsonBatteryCase> JsonBatteryCases { get; set; } = new List<JsonBatteryCase>();
        [JsonProperty("Ammeters")]
        public List<JsonAmmeter> JsonAmmeters { get; set; } = new List<JsonAmmeter>();
        [JsonProperty("Fans")]
        public List<JsonFan> JsonFans { get; set; } = new List<JsonFan>();
        [JsonProperty("Lampstands")]
        public List<JsonLampstand> JsonLampstands { get; set; } = new List<JsonLampstand>();
        [JsonProperty("Resistors")]
        public List<JsonResistor> JsonResistors { get; set; } = new List<JsonResistor>();
        [JsonProperty("Rheostats")]
        public List<JsonRheostat> JsonRheostats { get; set; } = new List<JsonRheostat>();
        [JsonProperty("Switchs")]
        public List<JsonSwitch> JsonSwitchs { get; set; } = new List<JsonSwitch>();
        [JsonProperty("Voltmeters")]
        public List<JsonVoltmeter> JsonVoltmeters { get; set; } = new List<JsonVoltmeter>();
        [JsonProperty(Consts.Json_Wires)]
        public List<JsonWire> Wires { get; set; } = new List<JsonWire>();

        private static T Build<T>(Element element) where T : JsonElement, new()
        {
            T je = new T();
            je.Type = element.Type;
            je.Name = element.SymbolName;
            je.X = element.WorldPoint.X;
            je.Y = element.WorldPoint.Y;
            je.Fault = element.Fault;
            return je;
        }

        private static List<JsonPointF> PointFList2JsonPointFList(List<PointF> pfs)
        {
            List<JsonPointF> jps = new List<JsonPointF>();
            if (pfs != null && pfs.Count > 0)
            {
                foreach (PointF point in pfs)
                {
                    jps.Add(new JsonPointF() { X = point.X, Y = point.Y });
                }
            }
            return jps;
        }

        private static List<PointF> JsonPointFList2PointFList(List<JsonPointF> jps)
        {
            List<PointF> pfs = new List<PointF>();
            foreach (JsonPointF json in jps)
            {
                pfs.Add(new PointF(json.X, json.Y));
            }
            return pfs;
        }

        public static string GetJsonStr(Workbench bench)
        {
            if (bench == null) return null;

            JsonWorkbench jwb = new JsonWorkbench();
            jwb.IsShowElementName = bench.IsShowElementName;
            jwb.IsShowWireName = bench.IsShowWireName;
            jwb.IsAutoAnalyze = bench.IsAutoAnalyze;
            jwb.IsLocked = bench.IsLocked;
            foreach (Element element in bench.Elements)
            {
                switch (element.Type)
                {
                    case ComponentType.Ammeter:
                        JsonAmmeter ja = Build<JsonAmmeter>(element);
                        ja.Scale = ((Ammeter)element).Scale;
                        jwb.JsonAmmeters.Add(ja);
                        break;
                    case ComponentType.BatteryCase:
                        JsonBatteryCase jbc = Build<JsonBatteryCase>(element);
                        jbc.Voltage = ((BatteryCase)element).Voltage;
                        jwb.JsonBatteryCases.Add(jbc);
                        break;
                    case ComponentType.Fan:
                        JsonFan jf = Build<JsonFan>(element);
                        Fan f = (Fan)element;
                        jf.RatedVoltage = f._rv;
                        jf.Resistance = f._r;
                        jf.InductiveReactance = f._ir;
                        jwb.JsonFans.Add(jf);
                        break;
                    case ComponentType.Lampstand:
                        JsonLampstand jl = Build<JsonLampstand>(element);
                        Lampstand l = (Lampstand)element;
                        jl.RatedPower = l.RatedPower;
                        jl.RatedVoltage = l.RatedVoltage;
                        jwb.JsonLampstands.Add(jl);
                        break;
                    case ComponentType.Resistor:
                        JsonResistor jr = Build<JsonResistor>(element);
                        jr.Resistance = ((Resistor)element).Resistance;
                        jwb.JsonResistors.Add(jr);
                        break;
                    case ComponentType.Rheostat:
                        JsonRheostat jh = Build<JsonRheostat>(element);
                        Rheostat rh = (Rheostat)element;
                        jh.MaxResistance = rh.MaxResistance;
                        jh.VanePoint = rh.TheVane.WorldPoint;
                        jwb.JsonRheostats.Add(jh);
                        break;
                    case ComponentType.Switch:
                        JsonSwitch js = Build<JsonSwitch>(element);
                        Switch s = ((Switch)element);
                        js.Stat = s.Stat;
                        jwb.JsonSwitchs.Add(js);
                        break;
                    case ComponentType.Voltmeter:
                        JsonVoltmeter jv = Build<JsonVoltmeter>(element);
                        Voltmeter v = (Voltmeter)element;
                        jv.Scale = v.Scale;
                        jwb.JsonVoltmeters.Add(jv);
                        break;
                }
            }
            foreach (Wire wire in bench.Wires)
            {
                JsonWire jw = new JsonWire();
                jw.Junctions = new List<JsonJunction>();
                jw.Points = PointFList2JsonPointFList(wire.PointFs);
                jw.Name = wire.SymbolName;
                foreach (Junction junction in wire.Junctions)
                {
                    JsonJunction jj = new JsonJunction();
                    if (junction.T != null)
                    {
                        jj.TerminalKey = junction.T.Key;
                        jj.OwnerName = junction.T.Owner.SymbolName;
                    }
                    jj.IsMoved = junction.IsMoved;
                    jj.Point = new JsonPointF() { X = junction.X, Y = junction.Y };
                    jj.Area = junction.Area;
                    jw.Junctions.Add(jj);
                }
                jwb.Wires.Add(jw);
            }
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.SerializeObject(jwb);
        }

        public static void LoadFromJson(string json, Workbench bench)
        {
            if (string.IsNullOrEmpty(json) || bench == null) return;
            try
            {
                bench.ClearComponents();
                JsonSerializerSettings setting = new JsonSerializerSettings();

                setting.NullValueHandling = NullValueHandling.Ignore;
                JsonWorkbench wb = JsonConvert.DeserializeObject<JsonWorkbench>(json);
                if (wb != null)
                {
                    bench.IsShowElementName = wb.IsShowElementName;
                    bench.IsShowWireName = wb.IsShowWireName;
                    bench.IsLocked = wb.IsLocked;
                    bench.IsAutoAnalyze = wb.IsAutoAnalyze;
                    //System.Diagnostics.Debugger.Break();
                    foreach (JsonAmmeter ja in wb.JsonAmmeters)
                    {
                        Ammeter a = new Ammeter();
                        a.X = ja.X;
                        a.Y = ja.Y;
                        a.Fault = ja.Fault;
                        a.SymbolName = ja.Name;
                        a.Bench = bench;
                        a.Scale = ja.Scale;
                        bench.AddElement(a);
                    }
                    foreach (JsonBatteryCase jbc in wb.JsonBatteryCases)
                    {
                        BatteryCase bc = new BatteryCase();
                        bc.Voltage = jbc.Voltage;
                        bc.X = jbc.X;
                        bc.Y = jbc.Y;
                        bc.SymbolName = jbc.Name;
                        bc.Fault = jbc.Fault;
                        bench.AddElement(bc);
                    }
                    foreach (JsonFan jf in wb.JsonFans)
                    {
                        Fan f = new Fan();
                        f.X = jf.X;
                        f.Y = jf.Y;
                        f.SymbolName = jf.Name;
                        f.Fault = jf.Fault;
                        f._ir = jf.InductiveReactance;
                        f._r = jf.Resistance;
                        f._rv = jf.RatedVoltage;
                        bench.AddElement(f);
                    }
                    foreach (JsonLampstand jl in wb.JsonLampstands)
                    {
                        Lampstand l = new Lampstand();
                        l.X = jl.X;
                        l.Y = jl.Y;
                        l.Fault = jl.Fault;
                        l.SymbolName = jl.Name;
                        l.RatedPower = jl.RatedPower;
                        l.RatedVoltage = jl.RatedVoltage;
                        bench.AddElement(l);
                    }
                    foreach (JsonResistor jr in wb.JsonResistors)
                    {
                        Resistor r = new Resistor();
                        r.X = jr.X;
                        r.Y = jr.Y;
                        r.Fault = jr.Fault;
                        r.Resistance = jr.Resistance;
                        r.SymbolName = jr.Name;
                        bench.AddElement(r);
                    }
                    foreach (JsonRheostat jrh in wb.JsonRheostats)
                    {
                        Rheostat rh = new Rheostat();
                        rh.X = jrh.X;
                        rh.Y = jrh.Y;
                        rh.Fault = jrh.Fault;
                        rh.SymbolName = jrh.Name;
                        rh.MaxResistance = jrh.MaxResistance;
                        rh.TheVane.X = jrh.VanePoint.X;
                        bench.AddElement(rh);
                    }
                    foreach (JsonSwitch jw in wb.JsonSwitchs)
                    {
                        Switch s = new Switch();
                        s.X = jw.X;
                        s.Y = jw.Y;
                        s.Fault = jw.Fault;
                        s.SymbolName = jw.Name;
                        s.Stat = jw.Stat;
                        bench.AddElement(s);
                    }
                    foreach (JsonVoltmeter js in wb.JsonVoltmeters)
                    {
                        Voltmeter v = new Voltmeter();
                        v.X = js.X;
                        v.Y = js.Y;
                        v.Fault = js.Fault;
                        v.SymbolName = js.Name;
                        v.Scale = js.Scale;
                        bench.AddElement(v);
                    }
                    foreach (JsonWire jsonWire in wb.Wires)
                    {
                        Wire w = new Wire();
                        w.Bench = bench;
                        w.PointFs.AddRange(JsonPointFList2PointFList(jsonWire.Points));
                        w.SymbolName = jsonWire.Name;
                        foreach (JsonJunction jj in jsonWire.Junctions)
                        {
                            if (!string.IsNullOrEmpty(jj.OwnerName) || jj.OwnerName == "null")
                            {
                                foreach (Element element in bench.Elements)
                                {
                                    if (element.SymbolName == jj.OwnerName)
                                    {
                                        foreach (Terminal terminal in element.Terminals)
                                        {
                                            if (terminal.Key == jj.TerminalKey)
                                            {
                                                Junction j = new Junction(w, jj.Area, terminal);
                                                j.IsMoved = jj.IsMoved;
                                                w.Junctions.Add(j);
                                                
                                                terminal.AddJunction(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Junction j = new Junction(w, jj.Area);
                                j.X = jj.Point.X;
                                j.Y = jj.Point.Y;
                                Console.WriteLine(jj.Point);
                                j.IsMoved = jj.IsMoved;
                                w.Junctions.Add(j);
                                //System.Diagnostics.Debugger.Break();
                            }
                            
                        }
                        bench.Items.Add(w);
                    }
                    bench.Draw();
                    if (bench.IsAutoAnalyze)
                    {
                        bench.DoTurnOnCircuit();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
