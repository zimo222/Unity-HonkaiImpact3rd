using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginView : MonoBehaviour
{
    [SerializeField] private float topABoundary = 0.268f, bottomABoundary = -0.268f, scrollASpeed = 0.012f; 
    [SerializeField] private List<Transform> backgroundAImages = new List<Transform>();
    [SerializeField] private float topBBoundary = 0.3095f, bottomBBoundary = -0.3095f, scrollBSpeed = 0.008f;
    [SerializeField] private List<Transform> backgroundBImages = new List<Transform>();
    [SerializeField] private float topCBoundary = 0.1941f, bottomCBoundary = -0.1941f, scrollCSpeed = 0.040f;
    [SerializeField] private List<Transform> backgroundCImages = new List<Transform>();
    [SerializeField] private float topDBoundary = 0.02f, bottomDBoundary = -0.08f, scrollDSpeed = 0.006f;
    [SerializeField] private Transform backgroundDImages;

    // Start is called before the first frame 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ScrollBackground();
        CheckAndResetPositions();
    }

    // 滚动背景
    private void ScrollBackground()
    {
        float moveAmount = scrollASpeed * Time.deltaTime;
        foreach (Transform image in backgroundAImages)
        {
            Vector3 newPosition = image.position;
            newPosition.y -= moveAmount;
            image.position = newPosition;
        }

        float moveBmount = scrollBSpeed * Time.deltaTime;
        foreach (Transform image in backgroundBImages)
        {
            Vector3 newPosition = image.position;
            newPosition.y -= moveBmount;
            image.position = newPosition;
        }

        float moveCmount = scrollCSpeed * Time.deltaTime;
        foreach (Transform image in backgroundCImages)
        {
            Vector3 newPosition = image.position;
            newPosition.y -= moveCmount;
            image.position = newPosition;
        }

        float moveDmount = scrollDSpeed * Time.deltaTime;
        Vector3 newDPosition = backgroundDImages.position;
        newDPosition.y -= moveDmount;
        backgroundDImages.position = newDPosition;
    }

    // 检查并重置位置
    private void CheckAndResetPositions()
    {
        Transform topImage = null;
        float highestY = Mathf.NegativeInfinity;
        foreach (Transform image in backgroundAImages)
        {
            if (image.position.y > highestY)
            {
                highestY = image.position.y;
                topImage = image;
            }
        }
        foreach (Transform image in backgroundAImages)
        {
            if (image.position.y <= bottomABoundary)
            {
                Vector3 newPosition = image.position;
                newPosition.y = topImage.position.y + topABoundary;
                image.position = newPosition;
            }
        }

        topImage = null;
        highestY = Mathf.NegativeInfinity;
        foreach (Transform image in backgroundBImages)
        {
            if (image.position.y > highestY)
            {
                highestY = image.position.y;
                topImage = image;
            }
        }
        foreach (Transform image in backgroundBImages)
        {
            if (image.position.y <= bottomBBoundary)
            {
                Vector3 newPosition = image.position;
                newPosition.y = topImage.position.y + topBBoundary;
                image.position = newPosition;
            }
        }

        topImage = null;
        highestY = Mathf.NegativeInfinity;
        foreach (Transform image in backgroundCImages)
        {
            if (image.position.y > highestY)
            {
                highestY = image.position.y;
                topImage = image;
            }
        }
        foreach (Transform image in backgroundCImages)
        {
            if (image.position.y <= bottomCBoundary)
            {
                Vector3 newPosition = image.position;
                newPosition.y = topImage.position.y + topCBoundary;
                image.position = newPosition;
            }
        }

        if(backgroundDImages.position.y <= bottomDBoundary)
        {
            Vector3 newPosition = backgroundDImages.position;
            newPosition.y = topImage.position.y + topDBoundary;
            backgroundDImages.position = newPosition;
        }
    }


    // 登录成功后调用
    public void OnLoginSuccess()
    {
        if (backgroundAImages[0] != null) PlayerDataManager.LastTopQuadAYPosition = Mathf.Max(backgroundAImages[0].position.y, backgroundAImages[1].position.y);
        if (backgroundBImages[0] != null) PlayerDataManager.LastTopQuadAYPosition = Mathf.Max(backgroundBImages[0].position.y, backgroundBImages[1].position.y);
        if (backgroundCImages[0] != null) PlayerDataManager.LastTopQuadAYPosition = Mathf.Max(backgroundCImages[0].position.y, backgroundCImages[1].position.y);
        if (backgroundDImages != null) PlayerDataManager.LastTopQuadAYPosition = backgroundDImages.position.y;

        SceneManager.LoadScene("ToHomeScene");
    }
}
