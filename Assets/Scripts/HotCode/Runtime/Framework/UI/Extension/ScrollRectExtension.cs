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

        // 缓动滑动到指定Index的Item
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

        // 缓动滑动到指定Index的Item的左侧
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

            // 获取target中心的位置，如果pivot是0.5，那么anchorPos就是target的中心
            var targetCenterPos = targetAnchorPos + new Vector2(targetSize.x / 2 * (targetPivot.x - 0.5f), targetSize.y / 2 * (targetPivot.y - 0.5f));

            normalizedPosition = new Vector2(
                Mathf.Clamp01((targetCenterPos.x - viewportSize.x / 2) / (contentSize.x - viewportSize.x)),
                Mathf.Clamp01((targetCenterPos.y - viewportSize.y / 2) / (contentSize.y - viewportSize.y))
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

            float horizontalNormalizedPosition = 0f;
            if (contentSize.x > viewportSize.x)
            {
                horizontalNormalizedPosition = targetLeftEdgePosX / (contentSize.x - viewportSize.x);
            }

            float verticalNormalizedPosition = 0f;
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