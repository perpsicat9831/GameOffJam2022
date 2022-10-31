using System.Xml.Linq;
using AK.Wwise;
using AudioStudio.Components;
using AudioStudio.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace AudioStudio.Tools
{
    internal class AsComponentExportParser: AsSearchers
    {
        #region FieldExporters
        //-----------------------------------------------------------------------------------------
        public string ExportVector3(Vector3 vector)
        {
            return vector.x.ToString("n3") + ", " + vector.y.ToString("n3") + ", " + vector.z.ToString("n3");
        }

        public void ExportGameObjects(GameObject parent, GameObject[] gameObjects, XElement xGameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (!gameObject) continue;
                var xGameObject = new XElement("GameObject");
                xGameObject.SetAttributeValue("Path", GetGameObjectPath(gameObject.transform, parent.transform));
                xGameObjects.Add(xGameObject);
            }
        }

        public void ExportSpatialSettings(Component component, XElement xComponent)
        {
            var emitter = (AudioEmitter3D)component;
            var xSettings = new XElement("SpatialSettings");
            xSettings.SetAttributeValue("IsUpdatePosition", emitter.IsUpdatePosition);
            xSettings.SetAttributeValue("UpdateFrequency", emitter.UpdateFrequency);
            xSettings.SetAttributeValue("PositionOffset", ExportVector3(emitter.PositionOffset));
            xSettings.SetAttributeValue("IsEnvironmentAware", emitter.IsEnvironmentAware);
            xSettings.SetAttributeValue("EnvironmentSource", emitter.EnvironmentSource);
            xSettings.SetAttributeValue("StopOnDestroy", emitter.StopOnDestroy);
            xSettings.SetAttributeValue("Listeners", emitter.Listeners);
            xComponent.Add(xSettings);
        }

        public void ExportTriggerSettings(AsTriggerHandler component, XElement xComponent)
        {
            var xSettings = new XElement("TriggerSettings");
            xSettings.SetAttributeValue("SetOn", component.SetOn);
            xSettings.SetAttributeValue("PostFrom", component.PostFrom);
            xSettings.SetAttributeValue("MatchTags", component.MatchTags);
            xComponent.Add(xSettings);
        }

        public void ExportRTPC(RTPC rtpc, float valueScale, XElement xComponent)
        {
            if (!rtpc.IsValid()) return;
            var xParam = new XElement("RTPC");
            xParam.SetAttributeValue("Name", rtpc.Name);
            xParam.SetAttributeValue("Guid", rtpc.ObjectReference.Guid);
            xParam.SetAttributeValue("ValueScale", valueScale);
            xComponent.Add(xParam);
        }

        public void ExportAuxBusExt(AuxBusExt bus, XElement xComponent)
        {
            if (!bus.IsValid()) return;
            var xBus = new XElement("AuxBus");
            xBus.SetAttributeValue("Name", bus.Name);
            xBus.SetAttributeValue("Guid", bus.ObjectReference.Guid);
            xBus.SetAttributeValue("SendAmount", bus.SendAmount);
            xComponent.Add(xBus);
        }

        public void ExportAuxBusesExt(AuxBusExt[] buses, XElement xBuses)
        {
            if (buses == null) return;
            foreach (var bus in buses)
            {
                ExportAuxBusExt(bus, xBuses);
            }
        }

        public void ExportBanksExt(BankExt[] banks, XElement xBanks)
        {
            if (banks == null) return;
            foreach (var bank in banks)
            {
                if (!bank.IsValid()) continue;
                var xBank = new XElement("Bank");
                xBank.SetAttributeValue("Name", bank.Name);
                xBank.SetAttributeValue("Guid", bank.ObjectReference.Guid);
                xBank.SetAttributeValue("UnloadOnDisable", bank.UnloadOnDisable);
                xBank.SetAttributeValue("UseCounter", bank.UseCounter);
                var xEvents = new XElement("AudioEvents");
                if (bank.LoadFinishEvents != null)
                    ExportAudioEvents(bank.LoadFinishEvents, xEvents);
                xBank.Add(xEvents);
                xBanks.Add(xBank);
            }
        }

        public void ExportStates(ASState[] states, XElement xStates, string trigger)
        {
            if (states == null) return;
            foreach (var state in states)
            {
                if (!state.IsValid()) continue;
                var xState = new XElement("State");
                xState.SetAttributeValue("Trigger", trigger);
                xState.SetAttributeValue("Name", state.ChildName);
                xState.SetAttributeValue("Guid", state.ObjectReference.Guid);
                xState.SetAttributeValue("GroupName", state.GroupName);
                xState.SetAttributeValue("GroupGuid", state.GroupWwiseObjectReference ? state.GroupWwiseObjectReference.Guid : System.Guid.Empty);
                xStates.Add(xState);
            }
        }

        public void ExportStatesExt(StateExt[] states, XElement xStates, string trigger)
        {
            if (states == null) return;
            foreach (var state in states)
            {
                if (!state.IsValid()) continue;
                var xState = new XElement("State");
                xState.SetAttributeValue("Trigger", trigger);
                xState.SetAttributeValue("Name", state.ChildName);
                xState.SetAttributeValue("Guid", state.ObjectReference.Guid);
                xState.SetAttributeValue("GroupName", state.GroupName);
                xState.SetAttributeValue("GroupGuid", state.GroupWwiseObjectReference ? state.GroupWwiseObjectReference.Guid : System.Guid.Empty);
                xState.SetAttributeValue("ResetOnDisable", state.ResetOnDisable);
                xStates.Add(xState);
            }
        }

        public void ExportSwitches(AudioSwitch[] switches, XElement xSwitches, string trigger)
        {
            if (switches == null) return;
            foreach (var swc in switches)
            {
                if (!swc.IsValid()) continue;
                var xSwitch = new XElement("Switch");
                xSwitch.SetAttributeValue("Trigger", trigger);
                xSwitch.SetAttributeValue("Name", swc.ChildName);
                xSwitch.SetAttributeValue("Guid", swc.ObjectReference.Guid);
                xSwitch.SetAttributeValue("GroupName", swc.GroupName);
                xSwitch.SetAttributeValue("GroupGuid", swc.GroupWwiseObjectReference ? swc.GroupWwiseObjectReference.Guid : System.Guid.Empty);
                xSwitches.Add(xSwitch);
            }
        }

        public void ExportTrigger(Trigger tgr, XElement xTriggers, string trigger = "")
        {
            if (!tgr.IsValid()) return;
            var xTrigger = new XElement("MusicTrigger");
            xTrigger.SetAttributeValue("Trigger", trigger);
            xTrigger.SetAttributeValue("Name", tgr.Name);
            xTrigger.SetAttributeValue("Guid", tgr.ObjectReference.Guid);
            xTriggers.Add(xTrigger);
        }

        public void ExportAcousticTexture(AcousticTexture texture, XElement xComponent)
        {
            if (!texture.IsValid()) return;
            var xTexture = new XElement("AcousticTexture");
            xTexture.SetAttributeValue("Name", texture.Name);
            xTexture.SetAttributeValue("Guid", texture.ObjectReference.Guid);
            xComponent.Add(xTexture);
        }

        public void ExportEvent(AudioEvent evt, XElement xEvents, string trigger = "")
        {
            if (!evt.IsValid()) return;
            var xEvent = new XElement("AudioEvent");
            if (!string.IsNullOrEmpty(trigger))
                xEvent.SetAttributeValue("Trigger", trigger);
            xEvent.SetAttributeValue("Name", evt.Name);
            xEvent.SetAttributeValue("Guid", evt.ObjectReference.Guid);
            xEvents.Add(xEvent);
        }

        public void ExportAudioEvents(AudioEvent[] events, XElement xEvents, string trigger = "")
        {
            if (events == null) return;
            foreach (var evt in events)
            {
                ExportEvent(evt, xEvents, trigger);
            }
        }

        public void ExportAudioEventsExt(AudioEventExt[] events, XElement xComponent, string trigger)
        {
            if (events == null) return;
            foreach (var evt in events)
            {
                if (!evt.IsValid()) continue;
                var xEvent = new XElement("AudioEvent");
                xEvent.SetAttributeValue("Trigger", trigger);
                xEvent.SetAttributeValue("Name", evt.Name);
                xEvent.SetAttributeValue("Guid", evt.ObjectReference.Guid);
                xEvent.SetAttributeValue("StopOnDisable", evt.StopOnDisable);
                xEvent.SetAttributeValue("FadeOutTime", evt.FadeOutTime);
                xComponent.Add(xEvent);
            }
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region ComponentExporters        
        //-----------------------------------------------------------------------------------------
        public void AudioTagExporter(Component component, XElement xComponent)
        {
            var s = (AudioTag)component;
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("Tags", s.Tags);
            xComponent.Add(xSettings);
        }

        public void AudioListener3DExporter(Component component, XElement xComponent)
        {
            var s = (AudioListener3D)component;
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("Index", s.Index);
            xSettings.SetAttributeValue("UseSpatialAudio", s.UseSpatialAudio);
            xSettings.SetAttributeValue("PositionOffset", ExportVector3(s.PositionOffset));
            xSettings.SetAttributeValue("FollowCamera", s.FollowCamera);
            xSettings.SetAttributeValue("LockYaw", s.LockYaw);
            xSettings.SetAttributeValue("LockPitch", s.LockPitch);
            xSettings.SetAttributeValue("LockRoll", s.LockRoll);
            xComponent.Add(xSettings);
        }

        public void AudioRoomExporter(Component component, XElement xComponent)
        {
            var s = (AudioRoom)component;
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("WallOcclusion", s.WallOcclusion);
            xSettings.SetAttributeValue("Priority", s.Priority);
            xComponent.Add(xSettings);
            ExportEvent(s.RoomToneEvent, xComponent);
            ExportAuxBusExt(s.RoomReverb, xComponent);
        }

        public void ButtonSoundExporter(Component component, XElement xComponent)
        {
            var s = (ButtonSound)component;
            var xEvents = new XElement("AudioEvents");
            ExportAudioEvents(s.ClickEvents, xEvents, "Click");
            ExportEvent(s.PointerEnterEvent, xEvents, "PointerEnter");
            ExportEvent(s.PointerExitEvent, xEvents, "PointerExit");
            xComponent.Add(xEvents);
        }

        public void ColliderSoundExporter(Component component, XElement xComponent)
        {
            var s = (ColliderSound)component;
            ExportTriggerSettings(s, xComponent);
            ExportSpatialSettings(s, xComponent);
            ExportRTPC(s.CollisionForceRTPC, s.ValueScale, xComponent);
            var xEvents = new XElement("AudioEvents");
            ExportAudioEvents(s.EnterEvents, xEvents, "Enter");
            ExportAudioEvents(s.ExitEvents, xEvents, "Exit");
            xComponent.Add(xEvents);
        }

        public void DropdownSoundExporter(Component component, XElement xComponent)
        {
            var s = (DropdownSound)component;
            var xEvents = new XElement("AudioEvents");
            ExportAudioEvents(s.PopupEvents, xEvents, "Popup");
            ExportAudioEvents(s.ValueChangeEvents, xEvents, "ValueChange");
            ExportAudioEvents(s.CloseEvents, xEvents, "Close");
            xComponent.Add(xEvents);
        }

        public void EffectSoundExporter(Component component, XElement xComponent)
        {
            var s = (EffectSound)component;
            ExportSpatialSettings(s, xComponent);
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("DelayTime", s.DelayTime);
            xComponent.Add(xSettings);
            var xEvents = new XElement("AudioEvents");
            ExportAudioEventsExt(s.EnableEvents, xEvents, "Enable");
            ExportAudioEvents(s.DisableEvents, xEvents, "Disable");
            xComponent.Add(xEvents);
        }

        public void EventSoundExporter(Component component, XElement xComponent)
        {
            var s = (EventSound)component;
            var xUIAudioEvents = new XElement("UIAudioEvents");
            foreach (var evt in s.UIAudioEvents)
            {
                var xUIAudioEvent = new XElement("UIAudioEvent");
                xUIAudioEvent.SetAttributeValue("Action", evt.Action);
                ExportEvent(evt.AudioEvent, xUIAudioEvent, evt.TriggerType.ToString());
                xUIAudioEvents.Add(xUIAudioEvent);
            }
            xComponent.Add(xUIAudioEvents);
        }

        public void EmitterSoundExporter(Component component, XElement xComponent)
        {
            var s = (EmitterSound)component;
            ExportTriggerSettings(s, xComponent);
            ExportSpatialSettings(s, xComponent);
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("MultiPositionType", s.MultiPositionType);
            xSettings.SetAttributeValue("FadeOutTime", s.FadeOutTime);
            xSettings.SetAttributeValue("InitialDelay", s.InitialDelay);
            xSettings.SetAttributeValue("MinInterval", s.MinInterval);
            xSettings.SetAttributeValue("MaxInterval", s.MaxInterval);
            xSettings.SetAttributeValue("PlayMode", s.PlayMode);
            xSettings.SetAttributeValue("PauseIfInvisible", s.PauseIfInvisible);
            var positionString = "";
            foreach (var vector in s.MultiPositionArray)
            {
                positionString += ExportVector3(vector) + "/";
            }
            xSettings.SetAttributeValue("Positions", positionString);
            xComponent.Add(xSettings);

            var xEvents = new XElement("AudioEvents");
            ExportAudioEvents(s.AudioEvents, xEvents);
            xComponent.Add(xEvents);
        }

        public void LegacyAnimationSoundExporter(Component component, XElement xComponent)
        {
            var s = (LegacyAnimationSound)component;
            ExportSpatialSettings(s, xComponent);
            var xEvents = new XElement("AnimationAudioEvents");
            foreach (var animationAudioEvent in s.AudioEvents)
            {
                var xEvent = new XElement("AnimationAudioEvent");
                xEvent.SetAttributeValue("ClipName", animationAudioEvent.ClipName);
                xEvent.SetAttributeValue("Frame", animationAudioEvent.Frame);
                ExportEvent(animationAudioEvent.AudioEvent, xEvent);
                xEvents.Add(xEvent);
            }
            xComponent.Add(xEvents);
        }

        public void LoadBankExporter(Component component, XElement xComponent)
        {
            var s = (LoadBank)component;
            //ExportSpatialSettings(s, xComponent);
            ExportTriggerSettings(s, xComponent);
            var xBanks = new XElement("Banks");
            ExportBanksExt(s.Banks, xBanks);
            xComponent.Add(xBanks);
        }

        public void MenuSoundExporter(Component component, XElement xComponent)
        {
            var s = (MenuSound)component;
            var xEvents = new XElement("AudioEvents");
            ExportAudioEvents(s.OpenEvents, xEvents, "Open");
            ExportAudioEvents(s.CloseEvents, xEvents, "Close");
            xComponent.Add(xEvents);
        }

        public void MusicSwitchExporter(Component component, XElement xComponent)
        {
            var s = (MusicSwitch)component;
            ExportTriggerSettings(s, xComponent);
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("PlayLastMusic", s.PlayLastMusic);
            xComponent.Add(xSettings);
            var xEvents = new XElement("AudioEvents");
            ExportEvent(s.OnMusic, xEvents, "On");
            ExportEvent(s.OffMusic, xEvents, "Off");
            xComponent.Add(xEvents);
            var xTriggers = new XElement("MusicTriggers");
            ExportTrigger(s.OnTrigger, xTriggers, "On");
            ExportTrigger(s.OffTrigger, xTriggers, "Off");
            xComponent.Add(xTriggers);
        }

        public void ReverbZoneExporter(Component component, XElement xComponent)
        {
            var s = (ReverbZone)component;
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("Priority", s.Priority);
            xSettings.SetAttributeValue("IsDefault", s.IsDefault);
            xSettings.SetAttributeValue("ExcludeOthers", s.ExcludeOthers);
            ExportAuxBusExt(s.AuxBus, xComponent);
        }

        public void GlobalAuxSendExporter(Component component, XElement xComponent)
        {
            var s = (GlobalAuxSend)component;
            ExportTriggerSettings(s, xComponent);
            var xBuses = new XElement("AuxBuses");
            ExportAuxBusesExt(s.AuxBuses, xComponent);
            xComponent.Add(xBuses);
        }

        public void SetStateExporter(Component component, XElement xComponent)
        {
            var s = (SetState)component;
            ExportTriggerSettings(s, xComponent);
            var xStates = new XElement("States");
            ExportStatesExt(s.OnStates, xStates, "On");
            ExportStates(s.OffStates, xStates, "Off");
            xComponent.Add(xStates);
        }

        public void SetSwitchExporter(Component component, XElement xComponent)
        {
            var s = (SetSwitch)component;
            ExportTriggerSettings(s, xComponent);
            var xSwitches = new XElement("Switches");
            ExportSwitches(s.OnSwitches, xSwitches, "On");
            ExportSwitches(s.OffSwitches, xSwitches, "Off");
            xComponent.Add(xSwitches);
        }

        public void ScrollSoundExporter(Component component, XElement xComponent)
        {
            var s = (ScrollSound)component;
            ExportEvent(s.downScrollEvent, xComponent);
            ExportEvent(s.upScrollEvent, xComponent);
        }

        public void SliderSoundExporter(Component component, XElement xComponent)
        {
            var s = (SliderSound)component;
            ExportRTPC(s.ConnectedRTPC, s.ValueScale, xComponent);
            ExportEvent(s.DragEvent, xComponent);
        }

        public void SurfaceReflectorExporter(Component component, XElement xComponent)
        {
            var s = (SurfaceReflector)component;
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("Diffraction", s.EnableDiffraction);
            xSettings.SetAttributeValue("OnBoundaryEdges", s.EnableDiffractionOnBoundaryEdges);
            xComponent.Add(xSettings);
            ExportAcousticTexture(s.AcousticTexture, xComponent);
        }

        public void ToggleSoundExporter(Component component, XElement xComponent)
        {
            var s = (ToggleSound)component;
            var xEvents = new XElement("AudioEvents");
            ExportAudioEvents(s.ToggleOnEvents, xEvents, "ToggleOn");
            ExportAudioEvents(s.ToggleOffEvents, xEvents, "ToggleOff");
            xComponent.Add(xEvents);
        }

        public void TimelineSoundExporter(Component component, XElement xComponent)
        {
            var s = (TimelineSound)component;
            ExportSpatialSettings(s, xComponent);
            var xEmitters = new XElement("Emitters");
            ExportGameObjects(s.gameObject, s.Emitters, xEmitters);
            xComponent.Add(xEmitters);
        }

        public void AudioStateExporter(AudioState s, XElement xComponent)
        {
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("AudioState", s.AnimationAudioState.ToString());
            xSettings.SetAttributeValue("ResetStateOnExit", s.ResetStateOnExit);
            xComponent.Add(xSettings);

            var xEvents = new XElement("AudioEvents");
            ExportAudioEventsExt(s.EnterEvents, xEvents, "Enter");
            ExportAudioEvents(s.ExitEvents, xEvents, "Exit");
            xComponent.Add(xEvents);

            var xSwitches = new XElement("Switches");
            ExportSwitches(s.EnterSwitches, xSwitches, "Enter");
            ExportSwitches(s.ExitSwitches, xSwitches, "Exit");
            xComponent.Add(xSwitches);

            var xStates = new XElement("States");
            ExportStates(s.EnterStates, xStates, "Enter");
            ExportStates(s.ExitStates, xStates, "Exit");
            xComponent.Add(xStates);
        }

        public void WwiseTimelineClipExporter(WwiseTimelineClip component, TimelineClip clip, XElement xComponent)
        {
            var xSettings = new XElement("Settings");
            xSettings.SetAttributeValue("StartTime", clip.start.ToString("0.00"));
            xSettings.SetAttributeValue("Duration", clip.duration.ToString("0.00"));
            xSettings.SetAttributeValue("EmitterIndex", component.EmitterIndex);
            xComponent.Add(xSettings);

            var xEvents = new XElement("AudioEvents");
            ExportAudioEventsExt(component.StartEvents, xEvents, "Start");
            ExportAudioEvents(component.EndEvents, xEvents, "End");
            xComponent.Add(xEvents);

            var xStates = new XElement("States");
            ExportStatesExt(component.StartStates, xStates, "Start");
            ExportStates(component.EndStates, xStates, "End");
            xComponent.Add(xStates);
        }

        //-----------------------------------------------------------------------------------------
        #endregion
    }
}