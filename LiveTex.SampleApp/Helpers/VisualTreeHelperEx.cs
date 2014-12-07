using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LiveTex.SampleApp.Helpers
{
	internal static class VisualTreeHelperEx
	{
		public static T GetDescendant<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject == null)
			{
				return null;
			}

			var queue = new Queue<DependencyObject>();
			queue.Enqueue(dependencyObject);

			while (queue.Count > 0)
			{
				var element = queue.Dequeue();
				if (element is T
					&& !ReferenceEquals(element, dependencyObject))
				{
					return (T)element;
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(element, index));
				}
			}

			return null;
		}

		public static IEnumerable<T> GetDescendants<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject == null)
			{
				yield break;
			}

			var queue = new Queue<DependencyObject>();
			queue.Enqueue(dependencyObject);

			while (queue.Count > 0)
			{
				var element = queue.Dequeue();

				if (element is T
					&& !ReferenceEquals(element, dependencyObject))
				{
					yield return (T)element;
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(element, index));
				}
			}
		}

		public static IEnumerable<T> GetSelfAndDescendants<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject == null)
			{
				yield break;
			}

			var queue = new Queue<DependencyObject>();
			queue.Enqueue(dependencyObject);

			while (queue.Count > 0)
			{
				var element = queue.Dequeue();

				if (element is T)
				{
					yield return (T)element;
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(element, index));
				}
			}
		}

		public static T GetAncestor<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			var current = dependencyObject;

			while (current != null)
			{
				if (current is T)
				{
					return (T)current;
				}

				current = VisualTreeHelper.GetParent(current);
			}

			return null;
		}

		public static IEnumerable<T> GetAncestors<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			var current = dependencyObject;

			while (current != null)
			{
				if (current is T)
				{
					yield return (T)current;
				}

				current = VisualTreeHelper.GetParent(current);
			}
		}
	}
}
