using UnityEditor;
using UnityEngine.UIElements;
using VF.Inspector;

namespace VF.Feature {

public class Modes : BaseFeature<VF.Model.Feature.Modes> {
    public override void Generate(VF.Model.Feature.Modes config) {
        var physBoneResetter = CreatePhysBoneResetter(config.resetPhysbones, config.name);

        var layerName = config.name;
        var layer = manager.NewLayer(layerName);
        var off = layer.NewState("Off");
        if (physBoneResetter != null) off.Drives(physBoneResetter, true);
        var param = manager.NewInt(config.name, synced: true, saved: config.saved);
        manager.NewMenuToggle(config.name + "/Off", param, 0);
        var i = 1;
        foreach (var mode in config.modes) {
            var num = i++;
            var clip = LoadState(config.name+"_"+num, mode.state);
            var state = layer.NewState(""+num).WithAnimation(clip);
            if (physBoneResetter != null) state.Drives(physBoneResetter, true);
            if (config.securityEnabled) {
                var paramSecuritySync = manager.NewBool("SecurityLockSync");
                state.TransitionsFromAny().When(param.IsEqualTo(num).And(paramSecuritySync.IsTrue()));
                state.TransitionsToExit().When(param.IsNotEqualTo(num));
                state.TransitionsToExit().When(paramSecuritySync.IsFalse());
            } else {
                state.TransitionsFromAny().When(param.IsEqualTo(num));
                state.TransitionsToExit().When(param.IsNotEqualTo(num));
            }
            manager.NewMenuToggle(config.name + "/Mode " + num, param, num);
        }
    }

    public override string GetEditorTitle() {
        return "Prop with Modes";
    }

    public override VisualElement CreateEditor(SerializedProperty prop) {
        return Toggle.CreateEditor(prop, true, true, content =>
            content.Add(VRCFuryEditorUtils.List(prop.FindPropertyRelative("modes"),
                renderElement: (i,e) => VRCFuryStateEditor.render(e.FindPropertyRelative("state"), "Mode " + (i+1)))));
    }
}

}
