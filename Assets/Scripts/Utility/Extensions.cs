using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 扩展方法集合，为常用类型添加便利方法
/// </summary>
public static class Extensions
{
    #region Transform Extensions
    
    /// <summary>
    /// 重置Transform的位置、旋转和缩放
    /// </summary>
    public static void ResetTransform(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// 重置Transform的本地位置、旋转和缩放
    /// </summary>
    public static void ResetLocalTransform(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// 销毁所有子对象
    /// </summary>
    public static void DestroyAllChildren(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
    
    /// <summary>
    /// 获取所有子对象（不包括孙子对象）
    /// </summary>
    public static List<Transform> GetDirectChildren(this Transform transform)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
        return children;
    }
    
    /// <summary>
    /// 查找指定名称的子对象（递归查找）
    /// </summary>
    public static Transform FindChildRecursive(this Transform transform, string name)
    {
        if (transform.name == name)
            return transform;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform result = transform.GetChild(i).FindChildRecursive(name);
            if (result != null)
                return result;
        }
        
        return null;
    }
    
    #endregion
    
    #region GameObject Extensions
    
    /// <summary>
    /// 获取或添加组件
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }
    
    /// <summary>
    /// 安全地获取组件
    /// </summary>
    public static T SafeGetComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            return null;
        
        return gameObject.GetComponent<T>();
    }
    
    /// <summary>
    /// 设置游戏对象的层级
    /// </summary>
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
    
    /// <summary>
    /// 设置游戏对象及其子对象的激活状态
    /// </summary>
    public static void SetActiveRecursively(this GameObject gameObject, bool active)
    {
        gameObject.SetActive(active);
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActiveRecursively(active);
        }
    }
    
    #endregion
    
    #region Vector Extensions
    
    /// <summary>
    /// 将Vector3转换为Vector2（丢弃z分量）
    /// </summary>
    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }
    
    /// <summary>
    /// 将Vector2转换为Vector3（z分量为0）
    /// </summary>
    public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
    {
        return new Vector3(vector.x, vector.y, z);
    }
    
    /// <summary>
    /// 获取Vector3的水平距离（忽略y分量）
    /// </summary>
    public static float HorizontalDistance(this Vector3 from, Vector3 to)
    {
        Vector2 fromH = new Vector2(from.x, from.z);
        Vector2 toH = new Vector2(to.x, to.z);
        return Vector2.Distance(fromH, toH);
    }
    
    /// <summary>
    /// 限制Vector3在指定范围内
    /// </summary>
    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z)
        );
    }
    
    /// <summary>
    /// 随机化Vector3的各个分量
    /// </summary>
    public static Vector3 Randomize(this Vector3 vector, float range)
    {
        return new Vector3(
            vector.x + Random.Range(-range, range),
            vector.y + Random.Range(-range, range),
            vector.z + Random.Range(-range, range)
        );
    }
    
    #endregion
    
    #region Color Extensions
    
    /// <summary>
    /// 修改颜色的透明度
    /// </summary>
    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
    
    /// <summary>
    /// 从十六进制字符串创建颜色
    /// </summary>
    public static Color FromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        return Color.white;
    }
    
    /// <summary>
    /// 将颜色转换为十六进制字符串
    /// </summary>
    public static string ToHex(this Color color)
    {
        return ColorUtility.ToHtmlStringRGBA(color);
    }
    
    /// <summary>
    /// 获取颜色的反色
    /// </summary>
    public static Color Invert(this Color color)
    {
        return new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
    }
    
    #endregion
    
    #region Collection Extensions
    
    /// <summary>
    /// 从列表中随机获取一个元素
    /// </summary>
    public static T GetRandomElement<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);
        
        return list[Random.Range(0, list.Count)];
    }
    
    /// <summary>
    /// 从数组中随机获取一个元素
    /// </summary>
    public static T GetRandomElement<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);
        
        return array[Random.Range(0, array.Length)];
    }
    
    /// <summary>
    /// 打乱列表顺序
    /// </summary>
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// 检查列表是否为空或null
    /// </summary>
    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return list == null || list.Count == 0;
    }
    
    /// <summary>
    /// 安全地获取列表元素
    /// </summary>
    public static T SafeGet<T>(this List<T> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count)
            return default(T);
        
        return list[index];
    }
    
    /// <summary>
    /// 移除列表中的null元素
    /// </summary>
    public static void RemoveNulls<T>(this List<T> list) where T : class
    {
        list.RemoveAll(item => item == null);
    }
    
    /// <summary>
    /// 获取列表的随机子集
    /// </summary>
    public static List<T> GetRandomSubset<T>(this List<T> list, int count)
    {
        if (list == null || count <= 0)
            return new List<T>();
        
        List<T> shuffled = new List<T>(list);
        shuffled.Shuffle();
        
        return shuffled.Take(Mathf.Min(count, shuffled.Count)).ToList();
    }
    
    #endregion
    
    #region String Extensions
    
    /// <summary>
    /// 检查字符串是否为空或null
    /// </summary>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }
    
    /// <summary>
    /// 检查字符串是否为空白或null
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
    
    /// <summary>
    /// 截取字符串到指定长度
    /// </summary>
    public static string Truncate(this string str, int maxLength)
    {
        if (str.IsNullOrEmpty() || maxLength <= 0)
            return string.Empty;
        
        return str.Length <= maxLength ? str : str.Substring(0, maxLength);
    }
    
    /// <summary>
    /// 首字母大写
    /// </summary>
    public static string Capitalize(this string str)
    {
        if (str.IsNullOrEmpty())
            return str;
        
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }
    
    /// <summary>
    /// 移除字符串中的空白字符
    /// </summary>
    public static string RemoveWhitespace(this string str)
    {
        if (str.IsNullOrEmpty())
            return str;
        
        return new string(str.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
    
    #endregion
    
    #region Math Extensions
    
    /// <summary>
    /// 将值重新映射到新的范围
    /// </summary>
    public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }
    
    /// <summary>
    /// 检查值是否在指定范围内
    /// </summary>
    public static bool InRange(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    
    /// <summary>
    /// 检查值是否在指定范围内
    /// </summary>
    public static bool InRange(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }
    
    /// <summary>
    /// 获取两个值之间的百分比
    /// </summary>
    public static float GetPercentage(this float value, float min, float max)
    {
        if (Mathf.Approximately(max - min, 0f))
            return 0f;
        
        return Mathf.Clamp01((value - min) / (max - min));
    }
    
    /// <summary>
    /// 将角度标准化到0-360度范围
    /// </summary>
    public static float NormalizeAngle(this float angle)
    {
        angle = angle % 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }
    
    #endregion
    
    #region Time Extensions
    
    /// <summary>
    /// 将秒数转换为分:秒格式
    /// </summary>
    public static string ToMinutesSeconds(this float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{remainingSeconds:00}";
    }
    
    /// <summary>
    /// 将秒数转换为时:分:秒格式
    /// </summary>
    public static string ToHoursMinutesSeconds(this float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600f);
        int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
        return $"{hours:00}:{minutes:00}:{remainingSeconds:00}";
    }
    
    #endregion
    
    #region RectTransform Extensions
    
    /// <summary>
    /// 设置RectTransform的锚点
    /// </summary>
    public static void SetAnchor(this RectTransform rectTransform, Vector2 anchor)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
    }
    
    /// <summary>
    /// 设置RectTransform的锚点和位置
    /// </summary>
    public static void SetAnchorAndPosition(this RectTransform rectTransform, Vector2 anchor, Vector2 position)
    {
        rectTransform.SetAnchor(anchor);
        rectTransform.anchoredPosition = position;
    }
    
    /// <summary>
    /// 获取RectTransform的世界坐标边界
    /// </summary>
    public static Bounds GetWorldBounds(this RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        Vector3 min = corners[0];
        Vector3 max = corners[0];
        
        foreach (Vector3 corner in corners)
        {
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }
        
        return new Bounds((min + max) * 0.5f, max - min);
    }
    
    #endregion
}