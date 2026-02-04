using UnityEngine;
using System.Collections.Generic;

public class VerticalScrollingBackground : MonoBehaviour
{
    [Header("滚动设置")]
    [SerializeField] public float scrollSpeed = 100f; // 滚动速度（单位/秒）
    [SerializeField] private float topBoundary = 1400f; // 顶部边界
    [SerializeField] private float bottomBoundary = -1400f; // 底部边界

    [Header("图像设置")]
    [SerializeField] private List<Transform> backgroundImages = new List<Transform>(); // 五张背景图片的Transform
    private float imageHeight = 700f; // 单张图片的高度
    private List<float> initialYPositions = new List<float>(); // 初始Y位置记录

    void Start()
    {
        // 保存初始位置并排列图片
        initialYPositions.Clear();

        // 从上到下排列五张图片
        float currentY = topBoundary;
        for (int i = 0; i < backgroundImages.Count; i++)
        {
            Vector3 pos = backgroundImages[i].position;
            backgroundImages[i].position = new Vector3(pos.x, currentY, pos.z);
            initialYPositions.Add(currentY);

            // 每张图片间隔一个图片高度的距离
            currentY -= imageHeight;
        }
        //Debug.Log($"背景初始化完成，图片高度: {imageHeight}，总高度范围: {topBoundary} 到 {bottomBoundary}");
    }

    void Update()
    {
        ScrollBackground();
        CheckAndResetPositions();
    }

    /// <summary>
    /// 滚动背景
    /// </summary>
    private void ScrollBackground()
    {
        if (backgroundImages.Count != 5) return;

        float moveAmount = scrollSpeed * Time.deltaTime;

        foreach (Transform image in backgroundImages)
        {
            // 向下滚动（Y值减小）
            Vector3 newPosition = image.position;
            newPosition.y -= moveAmount;
            image.position = newPosition;
        }
    }

    /// <summary>
    /// 检查并重置位置
    /// </summary>
    private void CheckAndResetPositions()
    {
        if (backgroundImages.Count != 5) return;

        // 找出最上面的图片（Y值最大）
        Transform topImage = null;
        float highestY = Mathf.NegativeInfinity;

        foreach (Transform image in backgroundImages)
        {
            if (image.position.y > highestY)
            {
                highestY = image.position.y;
                topImage = image;
            }
        }

        // 检查每张图片是否需要重置
        foreach (Transform image in backgroundImages)
        {
            // 如果图片已经完全移出底部边界
            if (image.position.y <= bottomBoundary - imageHeight)
            {
                // 将其瞬移到当前最上面的图片的上方
                Vector3 newPosition = image.position;
                newPosition.y = topImage.position.y + imageHeight;
                image.position = newPosition;

                Debug.Log($"重置图片位置: {image.name} 移动到 Y={newPosition.y}");
            }
        }
    }
}