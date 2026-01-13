using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Framework
{
    /// <summary>
    /// ScrollRect扩展方法
    /// </summary>
    public static class ScrollRectExtension
    {
        private static Transform GetChildActive(this Transform transform, int index)
        {
            int count = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    if (count == index)
                    {
                        return child;
                    }
                    count++;
                }

            }

            return null;
        }

        /// <summary>
        /// 立即滑动指定Index的Item到ScrollRect的中心
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="index"></param>
        public static void ScrollItemToCenterImmediate(this ScrollRect scrollRect, int index)
        {

            if (!TryGetScrollViewItemCenterNormalizedPosition(scrollRect, index, out var normalizedPosition))
            {
                return;
            }

            scrollRect.normalizedPosition = normalizedPosition;
        }


        /// <summary>
        /// 立即滑动指定Index的Item到ScrollRect的左侧
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="index"></param>
        public static void ScrollItemToLeftImmediate(this ScrollRect scrollRect, int index)
        {
            if (!TryGetScrollViewItemLeftTopNormalizedPosition(scrollRect, index, out var normalizedPosition))
            {
                return;
            }

            scrollRect.normalizedPosition = normalizedPosition;
        }



        /// <summary>
        /// 缓动滑动到指定Index的Item
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="index"></param>
        /// <param name="duration"></param>
        public static void ScrollItemToCenter(this ScrollRect scrollRect, int index, float duration)
        {

            if (!TryGetScrollViewItemCenterNormalizedPosition(scrollRect, index, out var normalizedPosition))
            {
                return;
            }

            scrollRect.DOKill();
            if (scrollRect.horizontal)
            {
                scrollRect.DOHorizontalNormalizedPos(normalizedPosition.x, duration).SetLink(scrollRect.gameObject);
            }

            if (scrollRect.vertical)
            {
                scrollRect.DOVerticalNormalizedPos(normalizedPosition.y, duration).SetLink(scrollRect.gameObject);
            }

        }


        /// <summary>
        /// 缓动滑动到指定Index的Item的左侧
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="index"></param>
        /// <param name="duration"></param>
        public static void ScrollItemToLeft(this ScrollRect scrollRect, int index, float duration)
        {

            if (!TryGetScrollViewItemLeftTopNormalizedPosition(scrollRect, index, out var normalizedPosition))
            {
                return;
            }

            scrollRect.DOKill();
            if (scrollRect.horizontal)
            {
                scrollRect.DOHorizontalNormalizedPos(normalizedPosition.x, duration).SetLink(scrollRect.gameObject);
            }

            if (scrollRect.vertical)
            {
                scrollRect.DOVerticalNormalizedPos(normalizedPosition.y, duration).SetLink(scrollRect.gameObject);
            }

        }



        /// <summary>
        /// 获取指定Index的Item在ScrollView中心的NormalizedPosition
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="index"></param>
        /// <param name="normalizedPosition"></param>
        /// <returns></returns>
        private static bool TryGetScrollViewItemCenterNormalizedPosition(ScrollRect scrollRect, int index, out Vector2 normalizedPosition)
        {

            normalizedPosition = Vector2.zero;
            if (scrollRect.viewport == null || scrollRect.content == null)
            {
                Debug.LogError("ScrollView Content or Viewport is null");
                return false;
            }

            var targetTransform = scrollRect.content.GetChildActive(index) as RectTransform;
            if (targetTransform == null)
            {
                Debug.LogError("index out of range");
                return false;
            }

            var targetSize = targetTransform.rect.size;
            var targetAnchorPos = targetTransform.anchoredPosition;
            var targetPivot = targetTransform.pivot;

            var viewportSize = scrollRect.viewport.rect.size;
            var contentSize = scrollRect.content.rect.size;

            // 计算 target 左边缘的位置（考虑 pivot）
            var targetLeftEdge = targetAnchorPos.x - targetSize.x * targetPivot.x;
            // 计算 target 中心的 X 位置
            var targetCenterX = targetLeftEdge + targetSize.x / 2;

            // 计算 target 顶边缘的位置（考虑 pivot，Y轴向下为负）
            var targetTopEdge = targetAnchorPos.y + targetSize.y * (1 - targetPivot.y);
            // 计算 target 中心的 Y 位置
            var targetCenterY = targetTopEdge - targetSize.y / 2;

            // 水平方向：normalizedPosition.x = 0 表示最左，1 表示最右
            float horizontalNormalized = 0f;
            if (contentSize.x > viewportSize.x)
            {
                // 让 target 中心对齐 viewport 中心
                horizontalNormalized = (targetCenterX - viewportSize.x / 2) / (contentSize.x - viewportSize.x);
            }

            // 垂直方向：normalizedPosition.y = 1 表示最顶，0 表示最底
            // anchoredPosition.y 向下为负，所以需要取反
            float verticalNormalized = 1f;
            if (contentSize.y > viewportSize.y)
            {
                // 让 target 中心对齐 viewport 中心，注意 Y 轴方向
                verticalNormalized = 1f - (Mathf.Abs(targetCenterY) - viewportSize.y / 2) / (contentSize.y - viewportSize.y);
            }

            normalizedPosition = new Vector2(
              Mathf.Clamp01(horizontalNormalized),
              Mathf.Clamp01(verticalNormalized)
            );

            return true;
        }


        /// <summary>
        /// 获取指定Index的Item在ScrollView左侧的NormalizedPosition (垂直居中)
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="index"></param>
        /// <param name="normalizedPosition"></param>
        /// <returns></returns>
        private static bool TryGetScrollViewItemLeftTopNormalizedPosition(ScrollRect scrollRect, int index, out Vector2 normalizedPosition, int offsetCorrect = -10)
        {

            normalizedPosition = Vector2.zero;
            if (scrollRect.viewport == null || scrollRect.content == null)
            {
                Debug.LogError("ScrollView Content or Viewport is null");
                return false;
            }

            var targetTransform = scrollRect.content.GetChildActive(index) as RectTransform;
            if (targetTransform == null)
            {
                Debug.LogError("index out of range");
                return false;
            }

            var targetSize = targetTransform.rect.size;
            var targetAnchorPos = targetTransform.anchoredPosition;
            var targetPivot = targetTransform.pivot;

            // 加点像素，防止target的左上角被遮挡
            targetAnchorPos += new Vector2(offsetCorrect, offsetCorrect);
            var viewportSize = scrollRect.viewport.rect.size;
            var contentSize = scrollRect.content.rect.size;

            // 计算目标项左边缘的位置
            var targetLeftEdgePosX = targetAnchorPos.x - targetSize.x * targetPivot.x;

            // 垂直方向上，我们仍然尝试将项目居中
            var targetCenterPosY = targetAnchorPos.y + targetSize.y / 2 * (targetPivot.y - 0.5f);

            float horizontalNormalizedPosition = 0f; // 默认为0，即最左边
            if (contentSize.x > viewportSize.x)
            {
                horizontalNormalizedPosition = targetLeftEdgePosX / (contentSize.x - viewportSize.x);
            }

            float verticalNormalizedPosition = 0f; // 默认为0，即最顶部
            if (contentSize.y > viewportSize.y)
            {
                verticalNormalizedPosition = (targetCenterPosY - viewportSize.y / 2) / (contentSize.y - viewportSize.y);
            }

            normalizedPosition = new Vector2(
              Mathf.Clamp01(horizontalNormalizedPosition),
              Mathf.Clamp01(verticalNormalizedPosition)
            );

            return true;
        }
    }


}