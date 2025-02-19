using System.Collections;
using UnityEngine;
using TMPro;

public class RotateMatchGame : MonoBehaviour
{
    public Transform targetObject; // �^�[�Q�b�g�I�u�W�F�N�g
    public Rigidbody playerRigidbody; // �v���C���[�I�u�W�F�N�g��Rigidbody
    public TMP_Text scoreText;
    public TMP_Text resultText;
    private int score = 0;
    private Quaternion targetRotation;
    private bool isGameOver = false;
    private Vector3 previousMousePosition;

    public float rotationSpeed = 5f;
    public float damping = 0.95f;
    public float highAngularDrag = 5f;
    public float lowAngularDrag = 0.1f;

    private float startTime;
    void Start()
    {
        startTime = Time.time;
        GenerateNewTarget();
       // UpdateTimeText();
     
            //GenerateNewTarget();
            UpdateScoreText();
        
    }

    void Update()
    {
      

        if (Input.GetMouseButtonDown(0))
        {
            previousMousePosition = Input.mousePosition;
            playerRigidbody.angularDamping = lowAngularDrag; // ���C������������
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            if (delta.magnitude > 0)
            {
                float yaw = -delta.x * rotationSpeed; // X�����̈ړ��Ń��[��]
                float pitch = delta.y * rotationSpeed; // Y�����̈ړ��Ńs�b�`��]

                Vector3 torque = new Vector3(pitch, yaw, 0);
                playerRigidbody.AddTorque(torque);

                playerRigidbody.angularDamping = lowAngularDrag; // ���C������������
            }
            else
            {
                playerRigidbody.angularDamping = highAngularDrag; // ���C��傫������
            }

            previousMousePosition = Input.mousePosition;
        }
        else
        {
            playerRigidbody.angularDamping = lowAngularDrag; // ���C������������

            //playerRigidbody.angularDamping = highAngularDrag; // �}�E�X�����ꂽ�疀�C��傫������
        }
        float angleDifference = Quaternion.Angle(playerRigidbody.rotation, targetRotation);

        if (angleDifference < 25f&& !isGameOver)
        {
            ScorePoint();
        }
    }

    void FixedUpdate()
    {
        if (!isGameOver)
        {
            playerRigidbody.angularVelocity *= damping; // ������K�p���ď��X�Ɍ���
        }

      
    }

    void ScorePoint()
    {
        score++;
        UpdateScoreText();
        if (score >= 5)
        {
            GameOver();
        }
        else
        {
            GenerateNewTarget();
        }
    }

    void GenerateNewTarget()
    {
        targetRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        targetObject.rotation = targetRotation;
       // playerRigidbody.rotation = Quaternion.identity; // ������
        //playerRigidbody.angularVelocity = Vector3.zero;
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score+ "/5";
    }

    void UpdateTimeText()
    {
      //  float elapsedTime = Time.time - startTime;
      //  timeText.text = "Time: " + elapsedTime.ToString("F2") + " seconds";
    }



    void GameOver()
    {
        //resultText.gameObject.SetActive(true);
        isGameOver = true;
        float elapsedTime = Time.time - startTime;
        scoreText.text = "Thank you for playing!\nTime: " + elapsedTime.ToString("F2") + " seconds";
        //resultText.text = "Game Over! Time Taken: " + elapsedTime.ToString("F2") + " seconds";
    }
}
