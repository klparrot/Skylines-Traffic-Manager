comma := ,
MYDIR := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))
export MONO_PATH := $(MYDIR)/Mono/lib/mono/2.0
export MONO_CFG_DIR := $(MYDIR)/Mono/etc
export LD_LIBRARY_PATH := $(MYDIR)/Mono/lib$(if $(LD_LIBRARY_PATH),:$(LD_LIBRARY_PATH))

Cities := $(wildcard ~/Library/Application\ Support/Steam/steamapps/common/Cities_Skylines/Cities.app)

Reference := 
Reference += Assembly-CSharp
Reference += ColossalManaged
Reference += ICities
Reference += System
Reference += System.Core
Reference += System.Xml
Reference += UnityEngine

Compile := 
Compile += CustomCarAI.cs
Compile += CustomCargoTruckAI.cs
Compile += CustomHumanAI.cs
Compile += CustomNetTool.cs
Compile += CustomPathFind.cs
Compile += CustomPathManager.cs
Compile += CustomRoadAI.cs
Compile += CustomPassengerCarAI.cs
Compile += CustomTransportLineAI.cs
Compile += RedirectionHelper.cs
Compile += SerializableDataExtension.cs
Compile += TrafficLightsManual.cs
Compile += TrafficLightSimulation.cs
Compile += TrafficLightTool.cs
Compile += TrafficLightsTimed.cs
Compile += Log.cs
Compile += Mod.cs
Compile += TrafficPriority.cs
Compile += TrafficRoadRestrictions.cs
Compile += UIBase.cs
Compile += Properties/AssemblyInfo.cs
Compile += UITimedLights.cs
Compile += UITrafficManager.cs

EmbeddedResource := 
EmbeddedResource += Resources/light_1_1.png
EmbeddedResource += Resources/light_1_2.png
EmbeddedResource += Resources/light_1_3.png
EmbeddedResource += Resources/light_2_1.png
EmbeddedResource += Resources/light_2_2.png
EmbeddedResource += Resources/light_2_3.png
EmbeddedResource += Resources/light_3_1.png
EmbeddedResource += Resources/light_3_2.png
EmbeddedResource += Resources/light_3_3.png
EmbeddedResource += Resources/light_4_1.png
EmbeddedResource += Resources/light_4_2.png
EmbeddedResource += Resources/light_4_3.png
EmbeddedResource += Resources/light_5_1.png
EmbeddedResource += Resources/light_5_2.png
EmbeddedResource += Resources/light_5_3.png
EmbeddedResource += Resources/light_6_1.png
EmbeddedResource += Resources/light_6_2.png
EmbeddedResource += Resources/light_6_3.png
EmbeddedResource += Resources/light_counter.png
EmbeddedResource += Resources/light_mode.png
EmbeddedResource += Resources/light_yellow.png
EmbeddedResource += Resources/pedestrian_light_1.png
EmbeddedResource += Resources/pedestrian_light_2.png
EmbeddedResource += Resources/pedestrian_mode_1.png
EmbeddedResource += Resources/pedestrian_mode_2.png
EmbeddedResource += Resources/sign_none.png
EmbeddedResource += Resources/sign_priority.png
EmbeddedResource += Resources/sign_stop.png
EmbeddedResource += Resources/sign_yield.png

all: KiwiTLM.dll

warn: WARN := 4
warn: KiwiTLM.dll

clean:
	-rm -f KiwiTLM.dll

install: KiwiTLM.dll
	cp -a $^ ~/Library/Application\ Support/Colossal\ Order/Cities_Skylines/Addons/Mods/KiwiTLM/

Mono:
	ln -fns '$(Cities)/Contents/Frameworks/Mono' $@

NOWARN := -nowarn:108,109,114,168,169,219,414,649
KiwiTLM.dll: Mono $(Compile) $(EmbeddedResource)
	Mono/bin/gmcs $(if $(WARN),,$(NOWARN)) -unsafe+ -out:$@ -t:library -lib:'$(Cities)/Contents/Resources/Data/Managed' $(addprefix -r:,$(Reference)) $(Compile) $(foreach res,$(EmbeddedResource),-res:$(res)$(comma)KiwiManager.$(subst /,.,$(res)))

Resources/%.png: Resources/SVG/%.svg
	$(if $(ImageMagick),$(ImageMagick)/bin/)convert -background none $< +set date:create +set date:modify +set svg:comment $@


.PHONY: all clean install warn
