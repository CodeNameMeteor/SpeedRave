// Compile-only stand-ins for the Sewer Rave types SpeedRave uses (from Assembly-CSharp, Assembly-CSharp-firstpass
// and UnityEngine.UI). Signatures only - no behaviour. Add members here when the mod starts using new ones.
using UnityEngine;
public class SuperTextMesh : MonoBehaviour { public string text; }
public class FoodControl : MonoBehaviour {
    public int cheese, fruit; public bool haveKey, hasBottlecap, hasPyramid, hasMug, hasDuck, hasPizza, display;
    public GameObject canvas; public SuperTextMesh inventoryText;
    public GameObject key, ducky, pizza, mug, pyramid, bottlecap;
    public void RefreshValues() {} private void SaveGame() {} private void Start() {} private void Update() {}
}
public class PersistControl : MonoBehaviour {}
public class TitleScreenControler : MonoBehaviour { public GameObject titleButtons; public void StartGame() {} public void ClearSaveData() {} private void Start() {} private void Update() {} }
public class DoorBehavior : MonoBehaviour { public string sceneSelection; private void OnTriggerEnter(Collider other) {} private void Start() {} }
public class EndingTeleporter : MonoBehaviour { private void OnTriggerEnter(Collider other) {} private void Start() {} }
public class TrainMapScript : MonoBehaviour { private bool selectpause, selExit, selMovie, selPossum, selSnake; private void OnTriggerStay(Collider other) {} }
public class AppearChance : MonoBehaviour { private void Start() {} }
public class Billboard_Random : MonoBehaviour { private void Start() {} }
public class GetDialogue : MonoBehaviour { private void Start() {} }
public class MaterialChangeScript : MonoBehaviour { private void Start() {} }
public class RatColorScript : MonoBehaviour { private void Start() {} }
public class SelectNPCScript : MonoBehaviour { private void Start() {} }
public class SpawnPointScript : MonoBehaviour { private void Start() {} }
public class WalkUpDialogue : MonoBehaviour { private void Start() {} }
public class LoadPlayerUpgrades : MonoBehaviour { private void Start() {} }
public class TitleColor : MonoBehaviour { private SuperTextMesh titleText; private void Start() {} public void ChangeColor() {} }
namespace UnityEngine.UI { public class Image : MonoBehaviour { public Sprite sprite; } }
namespace UnityStandardAssets.Characters.FirstPerson {
    public class MouseLook { private bool m_cursorIsLocked; private Quaternion m_CharacterTargetRot, m_CameraTargetRot; private void InternalLockUpdate() {} }
    public class FirstPersonController : MonoBehaviour { private MouseLook m_MouseLook; private Camera m_Camera; private bool m_Jump; private Vector2 m_Input; private void RotateView() {} private void Update() {} private void GetInput(out float speed) { speed = 0; } }
}
