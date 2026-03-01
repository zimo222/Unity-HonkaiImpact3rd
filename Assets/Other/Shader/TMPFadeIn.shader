using System.Collections;
using UnityEngine;
using TMPro;

public class SimpleTypewriterEffect : MonoBehaviour
{
    [Header("基础设置")]
    [SerializeField] private bool enableTypewriter = true; // 是否启用逐字显示
    [SerializeField] private float charactersPerSecond = 30f;
    
    [Header("淡入效果")]
    [SerializeField] private bool enableFadeIn = false; // 是否启用淡入效果
    [SerializeField] private float fadeInDuration = 0.5f; // 淡入持续时间
    
    private TMP_Text tmpTextComponent;
    private string fullText;
    private Coroutine typewriterCoroutine;
    private Coroutine fadeInCoroutine;
    
    void Start()
    {
        tmpTextComponent = GetComponent<TMP_Text>();
        
        if (tmpTextComponent == null)
        {
            Debug.LogError("SimpleTypewriterEffect: 找不到TMP_Text组件!");
            return;
        }
        
        fullText = tmpTextComponent.text;
        
        // 根据开关决定是否启用逐字显示
        if (enableTypewriter)
        {
            StartTypewriter();
        }
        else
        {
            // 如果不启用逐字显示，直接显示完整文本
            tmpTextComponent.text = fullText;
            
            // 如果启用淡入效果，应用整体淡入
            if (enableFadeIn)
            {
                ApplyOverallFadeIn();
            }
        }
    }
    
    public void StartTypewriter()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
            
        tmpTextComponent.text = "";
        
        // 如果启用淡入效果，使用带淡入的逐字显示
        if (enableFadeIn)
        {
            typewriterCoroutine = StartCoroutine(TypeTextWithFadeIn());
        }
        else
        {
            typewriterCoroutine = StartCoroutine(TypeText());
        }
    }
    
    // 普通的逐字显示
    IEnumerator TypeText()
    {
        float delay = 1f / charactersPerSecond;
        
        foreach (char letter in fullText)
        {
            // 直接添加字符到文本
            tmpTextComponent.text += letter;
            
            // 等待指定时间
            yield return new WaitForSeconds(delay);
        }
        
        typewriterCoroutine = null; // 打字完成后清空协程引用
    }
    
    // 带淡入效果的逐字显示
    IEnumerator TypeTextWithFadeIn()
    {
        float delay = 1f / charactersPerSecond;
        
        foreach (char letter in fullText)
        {
            // 添加字符到文本
            tmpTextComponent.text += letter;
            
            // 获取最新添加的字符的索引
            int lastCharIndex = tmpTextComponent.text.Length - 1;
            
            // 对新字符应用淡入效果
            StartCoroutine(FadeInCharacter(lastCharIndex));
            
            // 等待指定时间
            yield return new WaitForSeconds(delay);
        }
        
        typewriterCoroutine = null; // 打字完成后清空协程引用
    }
    
    // 对单个字符应用淡入效果
    IEnumerator FadeInCharacter(int charIndex)
    {
        // 等待一帧确保文本已渲染
        yield return null;
        
        // 强制更新网格以获取字符信息
        tmpTextComponent.ForceMeshUpdate();
        
        TMP_TextInfo textInfo = tmpTextComponent.textInfo;
        
        // 确保字符索引有效
        if (charIndex >= textInfo.characterCount) yield break;
        
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        
        // 跳过不可见字符
        if (!charInfo.isVisible) yield break;
        
        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        
        Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
        Color32 currentColor = vertexColors[vertexIndex];
        
        // 初始状态：完全透明
        for (int j = 0; j < 4; j++)
        {
            vertexColors[vertexIndex + j] = new Color32(
                currentColor.r, 
                currentColor.g, 
                currentColor.b, 
                0
            );
        }
        
        // 更新网格
        tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        
        // 淡入过程
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            byte alphaByte = (byte)(alpha * 255);
            
            // 更新字符的透明度
            for (int j = 0; j < 4; j++)
            {
                vertexColors[vertexIndex + j] = new Color32(
                    currentColor.r, 
                    currentColor.g, 
                    currentColor.b, 
                    alphaByte
                );
            }
            
            // 更新网格
            tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            
            yield return null;
        }
        
        // 确保最终状态是完全不透明
        for (int j = 0; j < 4; j++)
        {
            vertexColors[vertexIndex + j] = new Color32(
                currentColor.r, 
                currentColor.g, 
                currentColor.b, 
                255
            );
        }
        
        tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    
    // 整体淡入效果
    private void ApplyOverallFadeIn()
    {
        if (fadeInCoroutine != null)
            StopCoroutine(fadeInCoroutine);
            
        fadeInCoroutine = StartCoroutine(OverallFadeIn());
    }
    
    // 整体淡入协程
    IEnumerator OverallFadeIn()
    {
        // 等待一帧确保文本已渲染
        yield return null;
        
        // 强制更新网格
        tmpTextComponent.ForceMeshUpdate();
        
        TMP_TextInfo textInfo = tmpTextComponent.textInfo;
        
        // 初始状态：完全透明
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            if (!charInfo.isVisible) continue;
            
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            Color32 currentColor = vertexColors[vertexIndex];
            
            for (int j = 0; j < 4; j++)
            {
                vertexColors[vertexIndex + j] = new Color32(
                    currentColor.r, 
                    currentColor.g, 
                    currentColor.b, 
                    0
                );
            }
        }
        
        tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        
        // 淡入过程
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            byte alphaByte = (byte)(alpha * 255);
            
            // 更新所有字符的透明度
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                if (!charInfo.isVisible) continue;
                
                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                
                Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
                Color32 currentColor = vertexColors[vertexIndex];
                
                for (int j = 0; j < 4; j++)
                {
                    vertexColors[vertexIndex + j] = new Color32(
                        currentColor.r, 
                        currentColor.g, 
                        currentColor.b, 
                        alphaByte
                    );
                }
            }
            
            // 更新网格
            tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            
            yield return null;
        }
        
        // 确保最终状态是完全不透明
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            if (!charInfo.isVisible) continue;
            
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            Color32 currentColor = vertexColors[vertexIndex];
            
            for (int j = 0; j < 4; j++)
            {
                vertexColors[vertexIndex + j] = new Color32(
                    currentColor.r, 
                    currentColor.g, 
                    currentColor.b, 
                    255
                );
            }
        }
        
        tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        fadeInCoroutine = null;
    }
    
    // 公共方法，用于动态控制是否启用逐字显示
    public void SetTypewriterEnabled(bool enabled)
    {
        enableTypewriter = enabled;
        
        if (enabled)
        {
            // 如果启用逐字显示，重新开始打字效果
            fullText = tmpTextComponent.text;
            StartTypewriter();
        }
        else
        {
            // 如果禁用逐字显示，立即显示完整文本
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            tmpTextComponent.text = fullText;
            
            // 如果启用淡入效果，应用整体淡入
            if (enableFadeIn)
            {
                ApplyOverallFadeIn();
            }
        }
    }
    
    // 公共方法，用于动态控制是否启用淡入效果
    public void SetFadeInEnabled(bool enabled)
    {
        enableFadeIn = enabled;
        
        // 如果启用淡入效果，重新应用当前模式下的淡入
        if (enabled)
        {
            if (enableTypewriter)
            {
                // 逐字显示模式下重新开始打字效果
                fullText = tmpTextComponent.text;
                StartTypewriter();
            }
            else
            {
                // 整体模式下应用整体淡入
                ApplyOverallFadeIn();
            }
        }
    }
    
    public void SkipToEnd()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = null;
        }
        
        tmpTextComponent.text = fullText;
        
        // 确保文本完全不透明
        tmpTextComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmpTextComponent.textInfo;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            if (!charInfo.isVisible) continue;
            
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            Color32 currentColor = vertexColors[vertexIndex];
            
            for (int j = 0; j < 4; j++)
            {
                vertexColors[vertexIndex + j] = new Color32(
                    currentColor.r, 
                    currentColor.g, 
                    currentColor.b, 
                    255
                );
            }
        }
        
        tmpTextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    
    public void Restart()
    {
        fullText = tmpTextComponent.text;
        
        if (enableTypewriter)
        {
            StartTypewriter();
        }
        else
        {
            tmpTextComponent.text = fullText;
            
            // 如果启用淡入效果，应用整体淡入
            if (enableFadeIn)
            {
                ApplyOverallFadeIn();
            }
        }
    }
    
    public bool IsTyping()
    {
        return typewriterCoroutine != null;
    }
    
    // 在编辑器中进行更改时调用
    void OnValidate()
    {
        // 如果正在运行，立即应用更改
        if (Application.isPlaying && tmpTextComponent != null)
        {
            SetTypewriterEnabled(enableTypewriter);
            
            // 如果淡入效果开关发生变化，也应用更改
            if (enableFadeIn != GetCurrentFadeInState())
            {
                SetFadeInEnabled(enableFadeIn);
            }
        }
    }
    
    // 辅助方法：获取当前淡入效果状态
    private bool GetCurrentFadeInState()
    {
        // 这里简化处理，实际使用时可能需要更复杂的状态跟踪
        return enableFadeIn;
    }
    
    void OnDisable()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = null;
        }
    }
}