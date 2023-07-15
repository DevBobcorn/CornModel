using UnityEngine;
using TMPro;

public class Viewer : MonoBehaviour
{
    [SerializeField] public float sensitivityX = 5F;
    [SerializeField] public float sensitivityY = 5F;
    [SerializeField] public float moveSpeed = 5F;
    [SerializeField] private TMP_Text viewerText;
    [SerializeField] private Test game;

    void Update()
    {
        if (game != null && game.IsPaused)
        {
            return;
        }

        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        Vector3 orgPivotEuler = transform.rotation.eulerAngles;

        float camYaw   = orgPivotEuler.y + x * sensitivityX;
        float camPitch = orgPivotEuler.x - y * sensitivityY;

        while (camPitch < 0F)
            camPitch += 360F;

        transform.rotation = Quaternion.Euler(
            camPitch > 180F ? Mathf.Clamp(camPitch, 271F, 360F) : Mathf.Clamp(camPitch, 0F, 89F),
            camYaw,
            0F
        );

        Vector3 moveLocal = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveLocal += Vector3.forward;

        if (Input.GetKey(KeyCode.A))
            moveLocal += Vector3.left;

        if (Input.GetKey(KeyCode.S))
            moveLocal += Vector3.back;

        if (Input.GetKey(KeyCode.D))
            moveLocal += Vector3.right;
        
        var moveGlobal = transform.TransformVector(moveLocal);

        if (Input.GetKey(KeyCode.Space))
            moveGlobal.y =  1F;
        else if (Input.GetKey(KeyCode.LeftShift))
            moveGlobal.y = -1F;
        else
            moveGlobal.y =  0F;
        
        if (moveGlobal.magnitude > 0F)
        {
            moveGlobal = moveGlobal.normalized;
            transform.position += moveGlobal * moveSpeed * Time.deltaTime;
        }

        if (viewerText is not null)
        {
            var viewRay = Camera.main.ViewportPointToRay(new(0.5F, 0.5F, 0F));

            RaycastHit viewHit;
            string blockStateInfo;
            if (Physics.Raycast(viewRay.origin, viewRay.direction, out viewHit, 10F))
                blockStateInfo = viewHit.collider.gameObject.name;
            else
                blockStateInfo = string.Empty;

            viewerText.text = $"FPS: {(int)(1 / Time.unscaledDeltaTime)}\n{blockStateInfo}";
        }

    }
}
