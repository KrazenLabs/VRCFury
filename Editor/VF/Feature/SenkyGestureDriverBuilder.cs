using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VF.Builder;
using VF.Feature.Base;
using VF.Inspector;
using VF.Model.Feature;

namespace VF.Feature {

public class SenkyGestureDriverBuilder : FeatureBuilder<SenkyGestureDriver> {
    [FeatureBuilderAction(1)]
    public void Apply() {
        var blinkActive = manager.NewBool("BlinkActive", def: true);
        var paramEmoteHappy = manager.NewBool("EmoteHappy", synced: true);
        var paramEmoteSad = manager.NewBool("EmoteSad", synced: true);
        var paramEmoteAngry = manager.NewBool("EmoteAngry", synced: true);
        var paramEmoteTongue = manager.NewBool("EmoteTongue", synced: true);
        // These don't actually need synced, but vrc gets annoyed that the menu is using an unsynced param
        var paramEmoteHappyLock = manager.NewBool("EmoteHappyLock", synced: true);
        manager.NewMenuToggle("Emote Lock/Happy", paramEmoteHappyLock);
        var paramEmoteSadLock = manager.NewBool("EmoteSadLock", synced: true);
        manager.NewMenuToggle("Emote Lock/Sad", paramEmoteSadLock);
        var paramEmoteAngryLock = manager.NewBool("EmoteAngryLock", synced: true);
        manager.NewMenuToggle("Emote Lock/Angry", paramEmoteAngryLock);
        var paramEmoteTongueLock = manager.NewBool("EmoteTongueLock", synced: true);
        manager.NewMenuToggle("Emote Lock/Tongue", paramEmoteTongueLock);

        var transitionTime = model.transitionTime >= 0 ? model.transitionTime : 0.1f;

        {
            var layer = manager.NewLayer("Eyes");
            var idle = layer.NewState("Idle");
            var closed = layer.NewState("Closed").WithAnimation(LoadState("eyesClosed", model.eyesClosed));
            var happy = layer.NewState("Happy").WithAnimation(LoadState("eyesHappy", model.eyesHappy));
            //var bedroom = layer.NewState("Bedroom").WithAnimation(loadClip("eyesBedroom", inputs.eyesBedroom));
            var sad = layer.NewState("Sad").WithAnimation(LoadState("eyesSad", model.eyesSad));
            var angry = layer.NewState("Angry").WithAnimation(LoadState("eyesAngry", model.eyesAngry));

            if (blinkActive != null) {
                idle.Drives(blinkActive, true);
                closed.Drives(blinkActive, false);
                happy.Drives(blinkActive, false);
                //bedroom.Drives(blinkActive, false)
                sad.Drives(blinkActive, false);
                angry.Drives(blinkActive, false);
            }

            //closed.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramOrifaceMouthRing.IsTrue());
            //closed.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramOrifaceMouthHole.IsTrue());
            happy.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteHappy.IsTrue());
            //bedroom.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(bedroom.IsTrue());
            sad.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteSad.IsTrue());
            angry.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteAngry.IsTrue());
            idle.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(Always());
        }

        {
            var layer = manager.NewLayer("Mouth");
            var idle = layer.NewState("Idle");
            var blep = layer.NewState("Blep").WithAnimation(LoadState("mouthBlep", model.mouthBlep));
            var suck = layer.NewState("Suck").WithAnimation(LoadState("mouthSuck", model.mouthSuck));
            var sad = layer.NewState("Sad").WithAnimation(LoadState("mouthSad", model.mouthSad));
            var angry = layer.NewState("Angry").WithAnimation(LoadState("mouthAngry", model.mouthAngry));
            var happy = layer.NewState("Happy").WithAnimation(LoadState("mouthHappy", model.mouthHappy));

            //suck.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramOrifaceMouthRing.IsTrue());
            //suck.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramOrifaceMouthHole.IsTrue());
            blep.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteTongue.IsTrue());
            happy.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteHappy.IsTrue());
            sad.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteSad.IsTrue());
            angry.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(paramEmoteAngry.IsTrue());
            idle.TransitionsFromAny().WithTransitionToSelf().WithTransitionDurationSeconds(transitionTime).When(Always());
        }

        {
            var layer = manager.NewLayer("Ears");
            var idle = layer.NewState("Idle");
            var back = layer.NewState("Back").WithAnimation(LoadState("earsBack", model.earsBack));

            idle.TransitionsTo(back).WithTransitionDurationSeconds(0.1f).When(paramEmoteSad.IsTrue());
            idle.TransitionsTo(back).WithTransitionDurationSeconds(0.1f).When(paramEmoteAngry.IsTrue());
            back.TransitionsTo(idle).WithTransitionDurationSeconds(0.1f).When(paramEmoteSad.IsFalse().And(paramEmoteAngry.IsFalse()));
        }

        createGestureTriggerLayer("Tongue", paramEmoteTongueLock, paramEmoteTongue, 4);
        createGestureTriggerLayer("Happy", paramEmoteHappyLock, paramEmoteHappy, 7);
        createGestureTriggerLayer("Sad", paramEmoteSadLock, paramEmoteSad, 6);
        createGestureTriggerLayer("Angry", paramEmoteAngryLock, paramEmoteAngry, 5);
    }

    private void createGestureTriggerLayer(string name, VFABool lockParam, VFABool triggerParam, int gestureNum) {
        var layer = manager.NewLayer("Gesture - " + name);
        var off = layer.NewState("Off");
        var on = layer.NewState("On");

        var GestureLeft = manager.NewInt("GestureLeft", usePrefix: false);
        var GestureRight = manager.NewInt("GestureRight", usePrefix: false);

        off.TransitionsTo(on).When(lockParam.IsTrue());
        off.TransitionsTo(on).When(GestureLeft.IsEqualTo(gestureNum));
        off.TransitionsTo(on).When(GestureRight.IsEqualTo(gestureNum));
        on.TransitionsTo(off)
            .When(lockParam.IsFalse()
            .And(GestureLeft.IsNotEqualTo(gestureNum))
            .And(GestureRight.IsNotEqualTo(gestureNum)));

        off.Drives(triggerParam, false);
        on.Drives(triggerParam, true);
    }

    public override string GetEditorTitle() {
        return "Senky Gesture Driver";
    }

    public override VisualElement CreateEditor(SerializedProperty prop) {
        var content = new VisualElement();

        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("eyesClosed"), "Eyes Closed"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("eyesHappy"), "Eyes Happy"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("eyesSad"), "Eyes Sad"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("eyesAngry"), "Eyes Angry"));

        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("mouthBlep"), "Mouth Blep"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("mouthSuck"), "Mouth Suck"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("mouthSad"), "Mouth Sad"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("mouthAngry"), "Mouth Angry"));
        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("mouthHappy"), "Mouth Happy"));

        content.Add(VRCFuryStateEditor.render(prop.FindPropertyRelative("earsBack"), "Ears Back"));
        
        var adv = new Foldout {
            text = "Advanced",
            value = false
        };
        adv.Add(new PropertyField(prop.FindPropertyRelative("transitionTime"), "Transition Time (s)"));
        adv.Add(new Label("-1 will use VRCFury recommended value"));
        content.Add(adv);

        return content;
    }
    
    public override bool AvailableOnProps() {
        return false;
    }
}

}