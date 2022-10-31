using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AK.Wwise;
using AudioStudio.Components;
using AudioStudio.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace AudioStudio.Tools
{
    internal class AsComponentImportParser: AsSearchers
    {
        #region FieldImporters
        //-----------------------------------------------------------------------------------------
        public T XmlToWwiseObject<T>(XElement xEvent) where T : BaseType, new()
        {
            var guid = AsScriptingHelper.StringToGuid(AsScriptingHelper.GetXmlAttribute(xEvent, "Guid"));
            var objectByGuid = AkWwiseProjectInfo.GetData().FindWwiseObject<T>(guid);
            if (objectByGuid != null)
            {
                var n = new T();
                n.SetupReference(objectByGuid.Name, guid);
                return n;
            }
            var name = AsScriptingHelper.GetXmlAttribute(xEvent, "Name");
            var objectByName = AkWwiseProjectInfo.GetData().FindWwiseObject<T>(name);
            if (objectByName != null)
            {
                var n = new T();
                n.SetupReference(name, objectByName.Guid);
                return n;
            }
            return null;
        }

        public AudioEventExt XmlToAudioEventExt(XElement xEvent)
        {
            var evt = XmlToWwiseObject<AudioEventExt>(xEvent);
            if (evt == null) return null;
            var n = new AudioEventExt
            {
                StopOnDisable = AsScriptingHelper.StringToBool(AsScriptingHelper.GetXmlAttribute(xEvent, "StopOnDisable")),
                FadeOutTime = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(xEvent, "FadeOutTime"))
            };
            n.SetupReference(evt.Name, evt.ObjectReference.Guid);
            return n;
        }

        public bool ImportEvent(ref AudioEvent audioEvent, XElement xComponent)
        {
            var x = xComponent.Element("AudioEvent");
            if (x == null)
            {
                if (!audioEvent.IsValid()) return false;
                audioEvent = new AudioEvent();
                return true;
            }
            var temp = XmlToWwiseObject<AudioEvent>(x);
            if (!audioEvent.Equals(temp))
            {
                audioEvent = temp;
                return true;
            }
            return false;
        }

        public bool ImportEvent(ref AudioEvent audioEvent, XElement xComponent, string trigger)
        {
            var xEvents = xComponent.Element("AudioEvents");
            if (xEvents == null) return false;
            var temp = new AudioEvent();
            foreach (var xEvent in xEvents.Elements())
            {
                if (AsScriptingHelper.GetXmlAttribute(xEvent, "Trigger") == trigger)
                {
                    temp = XmlToWwiseObject<AudioEvent>(xEvent);
                    break;
                }
            }
            if (!audioEvent.Equals(temp))
            {
                audioEvent = temp;
                return true;
            }
            return false;
        }

        public bool ImportEvents(ref AudioEvent[] audioEvents, XElement xComponent, string trigger = "")
        {
            var xEvents = xComponent.Element("AudioEvents");
            if (xEvents == null) return false;
            var audioEventsTemp = new List<AudioEvent>();
            foreach (var xEvent in xEvents.Elements())
            {
                if (string.IsNullOrEmpty(trigger))
                    audioEventsTemp.Add(XmlToWwiseObject<AudioEvent>(xEvent));
                else
                {
                    if (AsScriptingHelper.GetXmlAttribute(xEvent, "Trigger") == trigger)
                        audioEventsTemp.Add(XmlToWwiseObject<AudioEvent>(xEvent));
                }
            }
            if (!audioEvents.ToList().SequenceEqual(audioEventsTemp))
            {
                audioEvents = audioEventsTemp.ToArray();
                return true;
            }
            return false;
        }

        public bool ImportAudioEventsExt(ref AudioEventExt[] audioEventExts, XElement xComponent, string trigger)
        {
            var xEvents = xComponent.Element("AudioEvents");
            if (xEvents == null) return false;
            var audioEventExtsTemp = new List<AudioEventExt>();
            foreach (var xEvent in xEvents.Elements())
            {
                if (AsScriptingHelper.GetXmlAttribute(xEvent, "Trigger") == trigger)
                    audioEventExtsTemp.Add(XmlToAudioEventExt(xEvent));
            }
            if (!audioEventExts.ToList().SequenceEqual(audioEventExtsTemp))
            {
                audioEventExts = audioEventExtsTemp.ToArray();
                return true;
            }
            return false;
        }

        public T XmlToWwiseGroupObject<T>(XElement xComponent) where T : BaseGroupType, new()
        {
            var groupGuid = AsScriptingHelper.StringToGuid(AsScriptingHelper.GetXmlAttribute(xComponent, "GroupGuid"));
            var groupObjectByGuid = AkWwiseProjectInfo.GetData().FindWwiseGroupUnit<T>(groupGuid);
            if (groupObjectByGuid != null)
            {
                var childGuid = AsScriptingHelper.StringToGuid(AsScriptingHelper.GetXmlAttribute(xComponent, "Guid"));
                var childName = AsToolWrapper.FindWwiseGroupObjectChildName(childGuid, groupObjectByGuid);
                if (string.IsNullOrEmpty(childName)) return null;
                var n = new T();
                n.SetupReference(childName, childGuid);
                var groupRef = n.ObjectReference as WwiseGroupValueObjectReference;
                if (groupRef)
                {
                    groupRef.SetupGroupObjectReference(groupObjectByGuid.Name, groupGuid);
                    n.ObjectReference = groupRef;
                }
                return n;
            }

            var groupName = AsScriptingHelper.GetXmlAttribute(xComponent, "GroupName");
            var groupObjectByName = AkWwiseProjectInfo.GetData().FindWwiseGroupUnit<T>(groupName);
            if (groupObjectByName != null)
            {
                var childName = AsScriptingHelper.GetXmlAttribute(xComponent, "Name");
                var childGuid = AsToolWrapper.FindWwiseGroupObjectChildGuid(childName, groupObjectByName);
                if (childGuid == Guid.Empty) return null;
                var n = new T();
                n.SetupReference(childName, childGuid);
                var groupRef = n.ObjectReference as WwiseGroupValueObjectReference;
                if (groupRef)
                {
                    groupRef.SetupGroupObjectReference(groupName, groupObjectByName.Guid);
                    n.ObjectReference = groupRef;
                }
                return n;
            }

            return null;
        }

        public StateExt XmlToStateExt(XElement xState)
        {
            var state = XmlToWwiseGroupObject<StateExt>(xState);
            if (state == null) return null;
            var n = new StateExt
            {
                ResetOnDisable = AsScriptingHelper.StringToBool(AsScriptingHelper.GetXmlAttribute(xState, "ResetOnDisable"))
            };
            n.SetupReference(state.ChildName, state.ObjectReference.Guid);
            var groupRef = n.ObjectReference as WwiseGroupValueObjectReference;
            if (groupRef)
            {
                groupRef.SetupGroupObjectReference(state.GroupName, state.GroupWwiseObjectReference ? state.GroupWwiseObjectReference.Guid : Guid.Empty);
                n.ObjectReference = groupRef;
            }
            return n;
        }

        public bool ImportStates(ref ASState[] states, XElement xComponent, string trigger = "")
        {
            var ss = xComponent.Element("States");
            if (ss == null) return false;
            var xStates = ss.Elements("State");
            var statesTemp = new List<ASState>();
            foreach (var xState in xStates)
            {
                if (trigger != "")
                {
                    if (AsScriptingHelper.GetXmlAttribute(xState, "Trigger") == trigger)
                        statesTemp.Add(XmlToWwiseGroupObject<ASState>(xState));
                }
                else
                {
                    statesTemp.Add(XmlToWwiseGroupObject<ASState>(xState));
                }
            }
            if (!states.ToList().SequenceEqual(statesTemp))
            {
                states = statesTemp.ToArray();
                return true;
            }
            return false;
        }

        public bool ImportStatesExt(ref StateExt[] states, XElement xComponent, string trigger = "")
        {
            var ss = xComponent.Element("States");
            if (ss == null) return false;
            var xStates = ss.Elements("State");
            var statesTemp = new List<StateExt>();
            foreach (var xState in xStates)
            {
                if (trigger != "")
                {
                    if (AsScriptingHelper.GetXmlAttribute(xState, "Trigger") == trigger)
                        statesTemp.Add(XmlToStateExt(xState));
                }
                else
                    statesTemp.Add(XmlToStateExt(xState));
            }
            if (!states.ToList().SequenceEqual(statesTemp))
            {
                states = statesTemp.ToArray();
                return true;
            }
            return false;
        }

        public bool ImportSwitches(ref SwitchEx[] switches, XElement xComponent, string trigger = "")
        {
            var ss = xComponent.Element("Switches");
            if (ss == null) return false;
            var xSwitches = ss.Elements("Switch");
            var switchesTemp = new List<SwitchEx>();
            foreach (var xSwitch in xSwitches)
            {
                if (trigger != "")
                {
                    if (AsScriptingHelper.GetXmlAttribute(xSwitch, "Trigger") == trigger)
                        switchesTemp.Add(XmlToWwiseGroupObject<SwitchEx>(xSwitch));
                }
                else
                    switchesTemp.Add(XmlToWwiseGroupObject<SwitchEx>(xSwitch));
            }
            if (!switches.ToList().SequenceEqual(switchesTemp))
            {
                switches = switchesTemp.ToArray();
                return true;
            }
            return false;
        }

        public bool ImportRTPC(ref RTPCExt rtpc, out float valueScale, XElement xComponent)
        {
            var x = xComponent.Element("RTPC");
            if (x == null)
            {
                valueScale = 1f;
                if (!rtpc.IsValid()) return false;
                rtpc = new RTPCExt();
                return true;
            }
            valueScale = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(x, "ValueScale"));
            var temp = XmlToWwiseObject<RTPCExt>(x);
            if (!rtpc.Equals(temp))
            {
                rtpc = temp;
                return true;
            }
            return false;
        }

        public bool ImportTrigger(ref TriggerExt tgr, XElement xComponent, string trigger)
        {
            var xTriggers = xComponent.Element("MusicTriggers");
            if (xTriggers == null) return false;
            var temp = new TriggerExt();
            foreach (var xTrigger in xTriggers.Elements())
            {
                if (AsScriptingHelper.GetXmlAttribute(xTrigger, "Trigger") == trigger)
                {
                    temp = XmlToWwiseObject<TriggerExt>(xTrigger);
                    break;
                }
            }
            if (!tgr.Equals(temp))
            {
                tgr = temp;
                return true;
            }
            return false;
        }

        public BankExt XmlToBankExt(XElement xBank)
        {
            var bank = XmlToWwiseObject<Bank>(xBank);
            if (bank == null) return null;
            var n = new BankExt
            {
                UnloadOnDisable = AsScriptingHelper.StringToBool(AsScriptingHelper.GetXmlAttribute(xBank, "UnloadOnDisable")),
                UseCounter = AsScriptingHelper.StringToBool(AsScriptingHelper.GetXmlAttribute(xBank, "UseCounter"))
            };
            ImportEvents(ref n.LoadFinishEvents, xBank);
            n.SetupReference(bank.Name, bank.ObjectReference.Guid);
            return n;
        }

        public bool ImportBanksExt(ref BankExt[] banks, XElement xComponent)
        {
            var bs = xComponent.Element("Banks");
            if (bs == null) return false;
            var xBanks = bs.Descendants("Bank");
            var banksTemp = xBanks.Select(XmlToBankExt).ToArray();
            if (!banks.SequenceEqual(banksTemp))
            {
                banks = banksTemp;
                return true;
            }
            return false;
        }

        public AuxBusExt XmlToAuxBusExt(XElement xAuxBus)
        {
            var bus = XmlToWwiseObject<AuxBus>(xAuxBus);
            if (bus == null) return null;
            var n = new AuxBusExt
            {
                SendAmount = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(xAuxBus, "SendAmount"))
            };
            n.SetupReference(bus.Name, bus.ObjectReference.Guid);
            return n;
        }

        public bool ImportAuxBusExt(ref AuxBusExt bus, XElement xComponent)
        {
            var x = xComponent.Element("AuxBus");
            if (x == null)
            {
                if (!bus.IsValid()) return false;
                bus = new AuxBusExt();
                return true;
            }
            bus.SendAmount = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(x, "SendAmount"));
            var temp = XmlToAuxBusExt(x);
            if (!bus.Equals(temp))
            {
                bus = temp;
                return true;
            }
            return false;
        }

        public bool ImportAuxBusesExt(ref AuxBusExt[] buses, XElement xComponent)
        {
            var abs = xComponent.Element("AuxBuses");
            if (abs == null) return false;
            var xBuses = abs.Elements("AuxBus");
            var busesTemp = xBuses.Select(XmlToAuxBusExt).ToList();
            if (!buses.ToList().SequenceEqual(busesTemp))
            {
                buses = busesTemp.ToArray();
                return true;
            }
            return false;
        }

        public bool ImportAcousticTexture(ref AcousticTexture texture, XElement xComponent)
        {
            var xTexture = xComponent.Element("AcousticTexture");
            if (xTexture == null) return false;
            var temp = XmlToWwiseObject<AcousticTexture>(xTexture);
            if (!texture.Equals(temp))
            {
                texture = temp;
                return true;
            }
            return false;
        }

        public bool ImportTriggerSettings(AsTriggerHandler aph, XElement xComponent)
        {
            var xSettings = xComponent.Element("TriggerSettings");
            var modified = ImportEnum(ref aph.MatchTags, AsScriptingHelper.GetXmlAttribute(xSettings, "MatchTags"));
            modified |= ImportEnum(ref aph.SetOn, AsScriptingHelper.GetXmlAttribute(xSettings, "SetOn"));
            modified |= ImportEnum(ref aph.PostFrom, AsScriptingHelper.GetXmlAttribute(xSettings, "PostFrom"));
            return modified;
        }

        public bool ImportSpatialSettings(Component component, XElement xComponent)
        {
            var emitter = (AudioEmitter3D)component;
            var xSettings = xComponent.Element("SpatialSettings");
            var modified = ImportBool(ref emitter.StopOnDestroy, AsScriptingHelper.GetXmlAttribute(xSettings, "StopOnDestroy"));
            modified |= ImportBool(ref emitter.IsUpdatePosition, AsScriptingHelper.GetXmlAttribute(xSettings, "IsUpdatePosition"));
            modified |= ImportBool(ref emitter.IsEnvironmentAware, AsScriptingHelper.GetXmlAttribute(xSettings, "IsEnvironmentAware"));
            modified |= ImportEnum(ref emitter.UpdateFrequency, AsScriptingHelper.GetXmlAttribute(xSettings, "UpdateFrequency"));
            modified |= ImportString(ref emitter.Listeners, AsScriptingHelper.GetXmlAttribute(xSettings, "Listeners"));
            modified |= ImportEnum(ref emitter.EnvironmentSource, AsScriptingHelper.GetXmlAttribute(xSettings, "EnvironmentSource"));
            modified |= ImportVector3(ref emitter.PositionOffset, AsScriptingHelper.GetXmlAttribute(xSettings, "PositionOffset"));
            return modified;
        }

        public bool ImportGameObjects(GameObject parent, ref GameObject[] gameObjects, XElement xGameObjects)
        {
            var gameObjectsTemp = xGameObjects.Elements("GameObject").Select
                (xGameObject => GetGameObject(parent, AsScriptingHelper.GetXmlAttribute(xGameObject, "Path"))).Where(gameObject => gameObject).ToList();
            if (!gameObjects.ToList().SequenceEqual(gameObjectsTemp))
            {
                gameObjects = gameObjectsTemp.ToArray();
                return true;
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region ComponentImporters    
        //-----------------------------------------------------------------------------------------
        public bool AudioTagImporter(Component component, XElement xComponent)
        {
            var s = (AudioTag)component;
            var xSettings = xComponent.Element("Settings");
            return ImportEnum(ref s.Tags, AsScriptingHelper.GetXmlAttribute(xSettings, "Tags"));
        }

        public bool AudioListener3DImporter(Component component, XElement xComponent)
        {
            var s = (AudioListener3D)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportVector3(ref s.PositionOffset, AsScriptingHelper.GetXmlAttribute(xSettings, "PositionOffset"));
            modified |= ImportByte(ref s.Index, AsScriptingHelper.GetXmlAttribute(xSettings, "Index"));
            modified |= ImportBool(ref s.UseSpatialAudio, AsScriptingHelper.GetXmlAttribute(xSettings, "UseSpatialAudio"));
            modified |= ImportBool(ref s.FollowCamera, AsScriptingHelper.GetXmlAttribute(xSettings, "FollowCamera"));
            modified |= ImportBool(ref s.LockYaw, AsScriptingHelper.GetXmlAttribute(xSettings, "LockYaw"));
            modified |= ImportBool(ref s.LockPitch, AsScriptingHelper.GetXmlAttribute(xSettings, "LockPitch"));
            modified |= ImportBool(ref s.LockRoll, AsScriptingHelper.GetXmlAttribute(xSettings, "LockRoll"));
            return modified;
        }

        public bool AudioRoomImporter(Component component, XElement xComponent)
        {
            var s = (AudioRoom)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportFloat(ref s.WallOcclusion, AsScriptingHelper.GetXmlAttribute(xSettings, "WallOcclusion"));
            modified |= ImportInt(ref s.Priority, AsScriptingHelper.GetXmlAttribute(xSettings, "Priority"));
            modified |= ImportEvent(ref s.RoomToneEvent, xComponent);
            modified |= ImportAuxBusExt(ref s.RoomReverb, xComponent);
            return modified;
        }

        public bool ButtonSoundImporter(Component component, XElement xComponent)
        {
            var s = (ButtonSound)component;
            var modified = ImportEvents(ref s.ClickEvents, xComponent, "Click");
            modified |= ImportEvent(ref s.PointerEnterEvent, xComponent, "PointerEnter");
            modified |= ImportEvent(ref s.PointerExitEvent, xComponent, "PointerExit");
            return modified;
        }

        public bool ColliderSoundImporter(Component component, XElement xComponent)
        {
            var s = (ColliderSound)component;
            var modified = ImportTriggerSettings(s, xComponent);
            modified |= ImportSpatialSettings(s, xComponent);
            modified |= ImportRTPC(ref s.CollisionForceRTPC, out s.ValueScale, xComponent);
            modified |= ImportEvents(ref s.EnterEvents, xComponent, "Enter");
            modified |= ImportEvents(ref s.ExitEvents, xComponent, "Exit");
            return modified;
        }

        public bool DropdownSoundImporter(Component component, XElement xComponent)
        {
            var s = (DropdownSound)component;
            var modified = ImportEvents(ref s.ValueChangeEvents, xComponent, "ValueChange");
            modified |= ImportEvents(ref s.PopupEvents, xComponent, "Popup");
            modified |= ImportEvents(ref s.CloseEvents, xComponent, "Close");
            return modified;
        }

        public bool EffectSoundImporter(Component component, XElement xComponent)
        {
            var s = (EffectSound)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportSpatialSettings(s, xComponent);
            modified |= ImportFloat(ref s.DelayTime, AsScriptingHelper.GetXmlAttribute(xSettings, "DelayTime"));
            modified |= ImportAudioEventsExt(ref s.EnableEvents, xComponent, "Enable");
            modified |= ImportEvents(ref s.DisableEvents, xComponent, "Disable");
            return modified;
        }

        public bool EventSoundImporter(Component component, XElement xComponent)
        {
            var s = (EventSound)component;
            var xUIAudioEvents = xComponent.Element("UIAudioEvents");
            if (xUIAudioEvents == null) return false;
            var xUIAudioEventList = xUIAudioEvents.Elements("UIAudioEvent").ToList();
            
            if (s.UIAudioEvents.Length < xUIAudioEventList.Count)
            {
                s.UIAudioEvents = new UIAudioEvent[xUIAudioEventList.Count];
                for (int index = 0; index < xUIAudioEventList.Count(); index++)
                {
                    var uiAudioEvent = new UIAudioEvent();
                    ImportEnum(ref uiAudioEvent.TriggerType, AsScriptingHelper.
                        GetXmlAttribute(xUIAudioEventList[index].Element("AudioEvent"), "Trigger"));
                    ImportEnum(ref uiAudioEvent.Action, AsScriptingHelper.
                        GetXmlAttribute(xUIAudioEventList[index], "Action"));
                    ImportEvent(ref uiAudioEvent.AudioEvent, xUIAudioEventList[index]);
                    s.UIAudioEvents[index] = uiAudioEvent;
                }
                return true;
            }

            var modified = false;
            for (int index = 0; index < xUIAudioEventList.Count(); index++)
            {
                modified |= ImportEnum(ref s.UIAudioEvents[index].TriggerType, AsScriptingHelper.
                    GetXmlAttribute(xUIAudioEventList[index].Element("AudioEvent"), "Trigger"));
                modified |= ImportEnum(ref s.UIAudioEvents[index].Action, AsScriptingHelper.
                    GetXmlAttribute(xUIAudioEventList[index], "Action"));
                modified |= ImportEvent(ref s.UIAudioEvents[index].AudioEvent, xUIAudioEventList[index]);
            }
            return modified;
        }

        public bool EmitterSoundImporter(Component component, XElement xComponent)
        {
            var s = (EmitterSound)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportSpatialSettings(s, xComponent);
            modified |= ImportTriggerSettings(s, xComponent);
            modified |= ImportEnum(ref s.MultiPositionType, AsScriptingHelper.GetXmlAttribute(xSettings, "MultiPositionType"));
            modified |= ImportEnum(ref s.PlayMode, AsScriptingHelper.GetXmlAttribute(xSettings, "PlayMode"));
            modified |= ImportFloat(ref s.InitialDelay, AsScriptingHelper.GetXmlAttribute(xSettings, "InitialDelay"));
            modified |= ImportFloat(ref s.MinInterval, AsScriptingHelper.GetXmlAttribute(xSettings, "MinInterval"));
            modified |= ImportFloat(ref s.MaxInterval, AsScriptingHelper.GetXmlAttribute(xSettings, "MaxInterval"));
            modified |= ImportFloat(ref s.FadeOutTime, AsScriptingHelper.GetXmlAttribute(xSettings, "FadeOutTime"));
            modified |= ImportBool(ref s.PauseIfInvisible, AsScriptingHelper.GetXmlAttribute(xSettings, "PauseIfInvisible"));
            modified |= ImportEvents(ref s.AudioEvents, xComponent);
            modified |= ImportVector3List(ref s.MultiPositionArray, xSettings);
            return modified;
        }

        public bool LegacyAnimationSoundImporter(Component component, XElement xComponent)
        {
            var s = (LegacyAnimationSound)component;
            var modified = ImportSpatialSettings(s, xComponent);
            var xEvents = xComponent.Element("AnimationAudioEvents");
            if (xEvents == null) return false;
            var newEvents = new List<AnimationAudioEvent>();
            foreach (var xEvent in xEvents.Elements())
            {
                var audioEvent = new AudioEvent();
                ImportEvent(ref audioEvent, xEvent);
                var animationAudioEvent = new AnimationAudioEvent
                {
                    AudioEvent = audioEvent,
                    ClipName = AsScriptingHelper.GetXmlAttribute(xEvent, "ClipName")
                };
                ImportInt(ref animationAudioEvent.Frame, AsScriptingHelper.GetXmlAttribute(xEvent, "Frame"));
                newEvents.Add(animationAudioEvent);
            }
            if (!newEvents.SequenceEqual(s.AudioEvents))
            {
                s.AudioEvents = newEvents.ToArray();
                return true;
            }
            return modified;
        }

        public bool LoadBankImporter(Component component, XElement xComponent)
        {
            var s = (LoadBank)component;
            //var modified = ImportSpatialSettings(s, xComponent);
            var modified = ImportTriggerSettings(s, xComponent);
            modified |= ImportBanksExt(ref s.Banks, xComponent);
            return modified;
        }

        public bool MenuSoundImporter(Component component, XElement xComponent)
        {
            var s = (MenuSound)component;
            var modified = ImportEvents(ref s.OpenEvents, xComponent, "Open");
            modified |= ImportEvents(ref s.CloseEvents, xComponent, "Close");
            return modified;
        }

        public bool MusicSwitchImporter(Component component, XElement xComponent)
        {
            var s = (MusicSwitch)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportTriggerSettings(s, xComponent);
            modified |= ImportBool(ref s.PlayLastMusic, AsScriptingHelper.GetXmlAttribute(xSettings, "PlayLastMusic"));
            modified |= ImportEvent(ref s.OnMusic, xComponent, "On");
            modified |= ImportEvent(ref s.OffMusic, xComponent, "Off");
            modified |= ImportTrigger(ref s.OnTrigger, xComponent, "On");
            modified |= ImportTrigger(ref s.OffTrigger, xComponent, "Off");
            return modified;
        }

        public bool ReverbZoneImporter(Component component, XElement xComponent)
        {
            var s = (ReverbZone)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportByte(ref s.Priority, AsScriptingHelper.GetXmlAttribute(xSettings, "Priority"));
            modified |= ImportBool(ref s.IsDefault, AsScriptingHelper.GetXmlAttribute(xSettings, "IsDefault"));
            modified |= ImportBool(ref s.ExcludeOthers, AsScriptingHelper.GetXmlAttribute(xSettings, "ExcludeOthers"));
            modified |= ImportAuxBusExt(ref s.AuxBus, xComponent);
            return modified;
        }

        public bool GlobalAuxSendImporter(Component component, XElement xComponent)
        {
            var s = (GlobalAuxSend)component;
            var modified = ImportTriggerSettings(s, xComponent);
            modified |= ImportAuxBusesExt(ref s.AuxBuses, xComponent);
            return modified;
        }

        public bool SetStateImporter(Component component, XElement xComponent)
        {
            var s = (SetState)component;
            var modified = ImportTriggerSettings(s, xComponent);
            modified |= ImportStatesExt(ref s.OnStates, xComponent, "On");
            modified |= ImportStates(ref s.OffStates, xComponent, "Off");
            return modified;
        }

        public bool SetSwitchImporter(Component component, XElement xComponent)
        {
            var s = (SetSwitch)component;
            var modified = ImportTriggerSettings(s, xComponent);
            modified |= ImportSwitches(ref s.OnSwitches, xComponent, "On");
            modified |= ImportSwitches(ref s.OffSwitches, xComponent, "Off");
            return modified;
        }

        public bool ScrollSoundImporter(Component component, XElement xComponent)
        {
            var s = (ScrollSound)component;
            return ImportEvent(ref s.downScrollEvent, xComponent) &&
                   ImportEvent(ref s.upScrollEvent, xComponent);
        }

        public bool SliderSoundImporter(Component component, XElement xComponent)
        {
            var s = (SliderSound)component;
            var modified = ImportRTPC(ref s.ConnectedRTPC, out s.ValueScale, xComponent);
            modified |= ImportEvent(ref s.DragEvent, xComponent);
            return modified;
        }

        public bool SurfaceReflectorImporter(Component component, XElement xComponent)
        {
            var s = (SurfaceReflector)component;
            var xSettings = xComponent.Element("Settings");
            var modified = ImportBool(ref s.EnableDiffraction, AsScriptingHelper.GetXmlAttribute(xSettings, "Diffraction"));
            modified |= ImportBool(ref s.EnableDiffractionOnBoundaryEdges, AsScriptingHelper.GetXmlAttribute(xSettings, "OnBoundaryEdges"));
            modified |= ImportAcousticTexture(ref s.AcousticTexture, xComponent);
            return modified;
        }

        public bool TimelineSoundImporter(Component component, XElement xComponent)
        {
            var s = (TimelineSound)component;
            var modified = ImportSpatialSettings(s, xComponent);
            var xEmitters = xComponent.Element("Emitters");
            modified |= ImportGameObjects(s.gameObject, ref s.Emitters, xEmitters);
            return modified;
        }

        public bool ToggleSoundImporter(Component component, XElement xComponent)
        {
            var s = (ToggleSound)component;
            var modified = ImportEvents(ref s.ToggleOnEvents, xComponent, "ToggleOn");
            modified |= ImportEvents(ref s.ToggleOffEvents, xComponent, "ToggleOff");
            return modified;
        }

        public bool AudioStateImporter(AudioState audioState, XElement xComponent)
        {
            var xSettings = xComponent.Element("Settings");
            var modified = ImportEnum(ref audioState.AnimationAudioState, AsScriptingHelper.GetXmlAttribute(xSettings, "AudioState"));
            modified |= ImportBool(ref audioState.ResetStateOnExit, AsScriptingHelper.GetXmlAttribute(xSettings, "ResetStateOnExit"));
            modified |= ImportAudioEventsExt(ref audioState.EnterEvents, xComponent, "Enter");
            modified |= ImportEvents(ref audioState.ExitEvents, xComponent, "Exit");
            modified |= ImportSwitches(ref audioState.EnterSwitches, xComponent, "Enter");
            modified |= ImportSwitches(ref audioState.ExitSwitches, xComponent, "Exit");
            modified |= ImportStates(ref audioState.EnterStates, xComponent, "Enter");
            modified |= ImportStates(ref audioState.ExitStates, xComponent, "Exit");
            return modified;
        }

        public bool WwiseTimelineClipImporter(WwiseTimelineClip apa, TimelineClip clip, XElement xComponent)
        {

            var xSettings = xComponent.Element("Settings");
            var clipName = AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName");
            var modified = ImportInt(ref apa.EmitterIndex, AsScriptingHelper.GetXmlAttribute(xSettings, "EmitterIndex"));
            var start = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(xSettings, "StartTime"));
            if (clip.displayName != clipName)
            {
                clip.displayName = clipName;
                modified = true;
            }
            if (Math.Abs(clip.start - start) >= 0.01f)
            {
                clip.start = start;
                modified = true;
            }
            var duration = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(xSettings, "Duration"));
            if (Math.Abs(clip.duration - duration) >= 0.01f)
            {
                clip.duration = duration;
                modified = true;
            }
            modified |= ImportAudioEventsExt(ref apa.StartEvents, xComponent, "Start");
            modified |= ImportEvents(ref apa.EndEvents, xComponent, "End");
            modified |= ImportStatesExt(ref apa.StartStates, xComponent, "Start");
            modified |= ImportStates(ref apa.EndStates, xComponent, "End");
            return modified;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region TypeCast
        //-----------------------------------------------------------------------------------------

        protected static bool ImportString(ref string field, string s)
        {
            if (field != s)
            {
                field = s;
                return true;
            }
            return false;
        }

        protected static bool ImportFloat(ref float field, string s)
        {
            var value = AsScriptingHelper.StringToFloat(s);
            if (field != value)
            {
                field = value;
                return true;
            }
            return false;
        }

        protected static bool ImportByte(ref byte field, string s)
        {
            var value = AsScriptingHelper.StringToByte(s);
            if (field != value)
            {
                field = value;
                return true;
            }
            return false;
        }

        protected static bool ImportInt(ref int field, string s)
        {
            var value = AsScriptingHelper.StringToInt(s);
            if (field != value)
            {
                field = value;
                return true;
            }
            return false;
        }

        protected static bool ImportBool(ref bool field, string s)
        {
            var value = AsScriptingHelper.StringToBool(s);
            if (field != value)
            {
                field = value;
                return true;
            }
            return false;
        }

        protected static bool ImportVector3(ref Vector3 field, string s)
        {
            var value = AsScriptingHelper.StringToVector3(s);
            if (Mathf.Abs(field.magnitude - value.magnitude) > 0.001f)
            {
                field = value;
                return true;
            }
            return false;
        }

        protected static bool ImportVector3List(ref List<Vector3> vectors, XElement xComponent)
        {
            var positionStrings = AsScriptingHelper.GetXmlAttribute(xComponent, "Positions").Split('/');
            var newPositions = new List<Vector3>();
            for (var i = 0; i < positionStrings.Length - 1; i++)
            {
                newPositions.Add(AsScriptingHelper.StringToVector3(positionStrings[i]));
            }

            if (newPositions.Count != vectors.Count)
            {
                vectors = newPositions;
                return true;
            }
            for (var i = 0; i < vectors.Count; i++)
            {
                if (Mathf.Abs(newPositions[i].magnitude - vectors[i].magnitude) > 0.01f)
                {
                    vectors = newPositions;
                    return true;
                }
            }
            return false;
        }

        protected static bool ImportEnum<T>(ref T field, string xComponent) where T : struct, IComparable
        {
            try
            {
                var value = (T)Enum.Parse(typeof(T), xComponent);
                if (!field.Equals(value))
                {
                    field = value;
                    return true;
                }
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                Debug.LogError("Import failed: Can't find option " + xComponent + " in enum " + typeof(T).Name);
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------
        #endregion
    }
}