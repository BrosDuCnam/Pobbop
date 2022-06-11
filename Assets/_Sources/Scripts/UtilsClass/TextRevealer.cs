using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public static class TextRevealer
{
    public static IEnumerator RevealText(TextMeshProUGUI textComponent, string message, float delay, bool delayByLetter)
    {
        int alphaStep = 8;
        int currentAlphaStep = 0;
        
        int currentChar = 0;

        float delayPerStep = delay / message.Length;
        if (!delayByLetter)
        {
            delayPerStep = delay / message.Length / alphaStep;
        }

        string currentText = "";
        
        while (currentChar < message.Length+1)
        {
            if (message.Length <= currentChar || message[currentChar] == ' ')
            {
                currentAlphaStep = 0;
                currentChar += 1;
                continue;
            }
            
            float alpha = ((float)currentAlphaStep / alphaStep);
            string hex = ColorUtility.ToHtmlStringRGBA(new Color(1, 1, 1, alpha));

            StringBuilder sb = new StringBuilder(message);
            sb.Insert(currentChar, "<color=#" + hex + ">");
            
            
            if (currentChar != message.Length)
            {
                sb.Insert(currentChar + 17 + 1, "</color>"); // 17 = length of hex color tag
                
                sb.Insert(currentChar + 17 + 1 + 8 , "<color=#11111100>");
                sb.Insert(sb.Length, "</color>"); // 17 = length of hex color tag
            }
            else sb.Insert(currentChar + 17, "</color>"); // 17 = length of hex color tag
            
            textComponent.text = sb.ToString();

            currentAlphaStep += 1;
            if (currentAlphaStep >= alphaStep)
            {
                currentAlphaStep = 0;
                currentChar += 1;
            }
            
            yield return new WaitForSeconds(delayPerStep);
        }
    }
}