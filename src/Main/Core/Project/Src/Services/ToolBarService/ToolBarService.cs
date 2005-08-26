﻿// <file>
//     <copyright see="prj:///doc/copyright.txt">2002-2005 AlphaSierraPapa</copyright>
//     <license see="prj:///doc/license.txt">GNU General Public License</license>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ICSharpCode.Core
{
	public static class ToolbarService
	{
		static ToolStripRenderer renderer;

		public static ToolStripRenderer Renderer {
			get {
				return renderer;
			}
			set {
				if (renderer != value) {
					renderer = value;
					if (RendererChanged != null)
						RendererChanged(null, EventArgs.Empty);
				}
			}
		}
		
		public static event EventHandler RendererChanged;
		
		public static ToolStripItem[] CreateToolStripItems(object owner, AddInTreeNode treeNode)
		{
			return (ToolStripItem[])(treeNode.BuildChildItems(owner)).ToArray(typeof(ToolStripItem));
		}
		
		public static ToolStrip CreateToolStrip(object owner, AddInTreeNode treeNode)
		{
			ToolStrip toolStrip = new ToolStrip();
			if (Renderer != null) toolStrip.Renderer = Renderer;
			toolStrip.Items.AddRange(CreateToolStripItems(owner, treeNode));
			UpdateToolbar(toolStrip); // setting Visible is only possible after the items have been added
			new LanguageChangeWatcher(toolStrip);
			return toolStrip;
		}
		
		class LanguageChangeWatcher {
			ToolStrip toolStrip;
			public LanguageChangeWatcher(ToolStrip toolStrip) {
				this.toolStrip = toolStrip;
				toolStrip.Disposed += Disposed;
				ResourceService.LanguageChanged += OnLanguageChanged;
				RendererChanged += OnRendererChanged;
			}
			void OnLanguageChanged(object sender, EventArgs e) {
				ToolbarService.UpdateToolbarText(toolStrip);
			}
			void OnRendererChanged(object sender, EventArgs e) {
				toolStrip.Renderer = Renderer ?? new ToolStripProfessionalRenderer();
			}
			void Disposed(object sender, EventArgs e) {
				ResourceService.LanguageChanged -= OnLanguageChanged;
				RendererChanged -= OnRendererChanged;
			}
		}
		
		public static ToolStrip CreateToolStrip(object owner, string addInTreePath)
		{
			return CreateToolStrip(owner, AddInTree.GetTreeNode(addInTreePath));
		}
		
		public static ToolStrip[] CreateToolbars(object owner, string addInTreePath)
		{
			AddInTreeNode treeNode;
			try {
				treeNode = AddInTree.GetTreeNode(addInTreePath);
			} catch (TreePathNotFoundException) {
				return null;
				
			}
			List<ToolStrip> toolBars = new List<ToolStrip>();
			foreach (AddInTreeNode childNode in treeNode.ChildNodes.Values) {
				toolBars.Add(CreateToolStrip(owner, childNode));
			}
			return toolBars.ToArray();
		}
		
		public static void UpdateToolbar(ToolStrip toolStrip)
		{
			foreach (ToolStripItem item in toolStrip.Items) {
				if (item is IStatusUpdate) {
					((IStatusUpdate)item).UpdateStatus();
				}
			}
			//toolStrip.Refresh();
		}
		
		public static void UpdateToolbarText(ToolStrip toolStrip)
		{
			foreach (ToolStripItem item in toolStrip.Items) {
				if (item is IStatusUpdate) {
					((IStatusUpdate)item).UpdateText();
				}
			}
		}
	}
}
