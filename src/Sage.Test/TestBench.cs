﻿// <auto-generated>Marked as auto-generated so StyleCop will ignore BDD style tests</auto-generated>
/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
namespace Sage.Test
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Machine.Specifications;

	using log4net;

	/// <summary>
	/// This class serves to hold temporary code snippets that can be run using the test runner.
	/// </summary>
	[Tags(Categories.TestBench)]
	public class TestBench
	{
		static readonly ILog log = LogManager.GetLogger(typeof(TestBench).FullName);
		static readonly Dictionary<string, Item> items = new Dictionary<string, Item>();

		private Establish context = () =>
		{
			items["a1"] = new Item("a1");
			items["b2"] = new Item("b2");
			items["c1"] = new Item("c1");
			items["b1"] = new Item("b1");
			items["c2"] = new Item("c2", "d2", "b1");
			items["a2"] = new Item("a2", "d2");
			items["dd"] = new Item("dd", "a1", "a2");
			items["e2"] = new Item("e2");
			items["d1"] = new Item("d1", "b1", "c2");
			items["e1"] = new Item("e1");
			items["d2"] = new Item("d2");

			items["aa"] = new Item("aa", "a1", "a2");
			items["ee"] = new Item("ee", "a1", "a2");
			items["bb"] = new Item("bb", "b1", "b2", "a1", "c1");
			items["cc"] = new Item("cc", "c2", "c1");


			List<Item> ordered = new List<Item>(items.Values.ToList());

			int index = 0;
			while (index != ordered.Count - 1)
			{
				var curr = ordered[index];
				var swapIndex = -1;

				log.DebugFormat("{0} Current: {1}", index, curr.Name);

				if (curr.Dependencies.Count == 0)
				{
					index += 1;
					continue;
				}

				for (int i = index + 1; i < ordered.Count; i++)
				{
					if (curr.Dependencies.Contains(ordered[i].Name))
					{
						swapIndex = i;
						break;
					}
				}

				if (swapIndex != -1)
				{
					var swap = ordered[swapIndex];
					ordered.RemoveAt(swapIndex);
					ordered.Insert(index, swap);
				}
				else
					index += 1;
			}
		};

		private It TestOrderingLists = () =>
		{
			true.ShouldEqual(true);
		};

		private class ItemComparer : IComparer<Item>
		{
			private Func<Item, Item, int> compare;

			public ItemComparer(Func<Item, Item, int> compare)
			{
				this.compare = compare;
			}

			public int Compare(Item x, Item y)
			{
				return compare(x, y);
			}
		}

		private class Item
		{
			public Item(string name)
			{
				this.Name = name;
			}

			public Item(string name, params string[] items)
				: this(name)
			{
				this.Dependencies.AddRange(items);
			}

			public string Name;

			public List<string> Dependencies = new List<string>();

			public override string ToString()
			{
				return string.Format("{0} {1}", this.Name, this.Dependencies.Count == 0
					? string.Empty : "(" + string.Join(",", this.Dependencies) + ")");
			}
		}
	}
}