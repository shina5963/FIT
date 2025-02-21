using System.Collections;
using UnityEngine;
using TMPro;

using UnityEngine.UI;
using UnityEngine.EventSystems; // 🔵 UIの判定に必要
using System.Collections.Generic;
using static UnityEngine.GridBrushBase;

public class RotateMatchGame : MonoBehaviour
{
    public Transform targetObject; // �^�[�Q�b�g�I�u�W�F�N�g
    public Rigidbody playerRigidbody; // �v���C���[�I�u�W�F�N�g��Rigidbody
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public Slider scoreSlider; // 🟢 追加（スライダー）
    private int score = 0;
    private Quaternion targetRotation;
    private bool isGameOver = false;
    private Vector3 previousMousePosition;

    public float rotationSpeed = 5f;
    public float damping = 0.95f;
    public float highAngularDrag = 5f;
    public float lowAngularDrag = 0.1f;

    private float startTime;

    private float firstSuccessTime = -1f; // 🟢 1回目の成功時の時間
    void Start()
    {
        startTime = Time.time;
        GenerateNewTarget();
       // UpdateTimeText();
     
            //GenerateNewTarget();
          //  UpdateScoreText();
        UpdateUI(); // 🟢 変更（UI更新関数を呼ぶ）

    }

    private bool isSnapping = false;
    private float snapSpeed = 5f; // スナップのスピード
                                  // 🔵 ボタンの上にカーソルがあるか判定する関数
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0; // 何かしらの UI に当たっていたら true
    }
    bool isNotTouch = true;

  void RotateObject()
    {
        if (IsPointerOverUIObject()) return;
        if (Input.GetMouseButtonUp(0))
        {
            isNotTouch = false;
            lastTouchTime= Time.time;

        }

        if (Input.GetMouseButtonDown(0))
        {



            previousMousePosition = Input.mousePosition;
            playerRigidbody.angularDamping = lowAngularDrag; // 低い角速度減衰
        }
        if (isNotTouch) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            if (delta.magnitude > 0)
            {
                // 解像度に依存しない回転処理
                float yaw = -delta.x / Screen.width * 500f * rotationSpeed;
                float pitch = delta.y / Screen.height * 500f * rotationSpeed;


                Vector3 torque = new Vector3(pitch, yaw, 0);
                playerRigidbody.AddTorque(torque);
                playerRigidbody.angularDamping = lowAngularDrag;
            }
            else
            {
                playerRigidbody.angularDamping = highAngularDrag;
            }

            previousMousePosition = Input.mousePosition;
        }
        else
        {
            playerRigidbody.angularDamping = lowAngularDrag;
        }
    }
    float lastTouchTime;

    Vector3 torque;
    void Update()
    {
   
        RotateObject();

        if ( !isGameOver&& score>0)
            UpdateScoreText();

        float angleDifference = Quaternion.Angle(playerRigidbody.rotation, targetRotation);

        if (isGameOver)
        {
            if (Time.time - lastTouchTime > 10f)
                isNotTouch=true;
        }


           
        UpdateBackgroundColor(angleDifference);
      //  if (angleDifference < 25f && !isGameOver && !isSnapping)
        if (angleDifference < 25f  && !isSnapping&&!isNotTouch)
            {
            isSnapping = true; // スナップ開始
        }


        if (isSnapping)
        {
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, Time.deltaTime * snapSpeed);
            playerRigidbody.angularDamping = highAngularDrag;
            // スナップ完了判定
            if (Quaternion.Angle(playerRigidbody.rotation, targetRotation) < 5f)
            {
                playerRigidbody.rotation = targetRotation; // 最終的に完全一致
                isSnapping = false; // スナップ完了
                ScorePoint(); // スコア加算
            }
        }
    }

    // 🟢 カメラの背景色を変更する関数を追加
    private float currentIntensity = 0f; // 現在の背景色の明るさ
    public float colorLerpSpeed = 5f; // 色変化のスピード

    void UpdateBackgroundColor(float angleDifference)
    {
        
        // 角度差を 0（黒）～ 180（白）にマッピング
        float targetIntensity = Mathf.Clamp01(angleDifference / 180f);

        // 徐々に targetIntensity に近づくように補間
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * colorLerpSpeed);

        // 背景色を変更
        Camera.main.backgroundColor = new Color(currentIntensity, currentIntensity, currentIntensity);
    }


    void FixedUpdate()
    {
        if (!isGameOver)
        {
            playerRigidbody.angularVelocity *= damping; // ������K�p���ď��X�Ɍ���
        }

        // 🟢 ここでカメラの背景色を変更
        if (isNotTouch && Time.time - finishTime > 10f)
        {

            if (Mathf.FloorToInt(Time.time - finishTime) % 10 == 0)
            {
                torque = new Vector3(
               Random.Range(-1f, 1f),
               Random.Range(-1f, 1f),
               0).normalized; // X, Yの回転方向をランダムに変更（正規化）
                              // playerRigidbody.angularVelocity *= 0;
            }
            playerRigidbody.AddTorque(torque );
        }

    }
   public GameObject StartObject;
    public GameObject ThankYou;
    void ScorePoint()
    {

        if (firstSuccessTime < 0)
        {

            isGameOver = false;
            firstSuccessTime = Time.time; // 🟢 1回目の成功時のタイム記録
        }
        if (score == 0)
        {

            StartObject.SetActive(false);
            ThankYou.SetActive(false);
        }
            score++;
        UpdateUI(); // 🟢 変更（UI更新）
        if (score >= 6)
        {
            GameOver();
            isNotTouch = true;
            playerRigidbody.angularVelocity*=0; // ������K�p���ď��X�Ɍ���
            GenerateNewTarget();
            score = 0;
            StartObject.SetActive(true);
            ThankYou.SetActive(true);
            firstSuccessTime = -1;



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

    float elipsedTime;
    void UpdateScoreText()
    {
        // scoreText.text = "Score: " + score+ "/5";
        elipsedTime = Time.time - firstSuccessTime; // 🟢 1回目成功時からの経過時間

        scoreText.text = elipsedTime.ToString("F2");
    }

    void UpdateUI() // 🟢 追加（ゲージUIの更新）
    {
        //scoreText.text = "Score: " + score + "/5";
        scoreSlider.value = score; // 🟢 ゲージの更新
    }

    void UpdateTimeText()
    {
      //  float elapsedTime = Time.time - startTime;
      //  timeText.text = "Time: " + elapsedTime.ToString("F2") + " seconds";
    }

    float finishTime;

   void GameOver()
    {
        //resultText.gameObject.SetActive(true);
        Camera.main.backgroundColor = new Color(0,0,0);

        isGameOver = true;
      //  float elapsedTime = Time.time - startTime;
         finishTime = Time.time - firstSuccessTime; // 🟢 1回目成功時からの経過時間

        scoreText.text = finishTime.ToString("F2");
        //resultText.text = "Game Over! Time Taken: " + elapsedTime.ToString("F2") + " seconds";
    }
}
