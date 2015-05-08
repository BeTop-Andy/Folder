using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using C1.Silverlight.FlexGrid;

using HuaweiSoftware.ZJNET.CommonSL;

namespace HuaweiSoftware.Folder.FolderUI
{
	public partial class MainPage : UserControl
	{
		private ObservableCollection<string> m_Extensions;		// 后缀名的集合

		private FolderHelper m_FolderHelper;					// 操作数据库的对象

		public MainPage()
		{
			InitializeComponent();

			m_FolderHelper = new FolderHelper();

			m_Extensions = new ObservableCollection<string>();
			ddlstExtension.ItemsSource = m_Extensions;
			m_Extensions.Add("ALL");

			CellHandler cellHandler = new CellHandler();

			flxFiles.CellFactory = cellHandler;
			flxFolder.CellFactory = cellHandler;

			cellHandler.MyRowHeader = CreateRowHeader;
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			flxFolder.ItemsSource = null;
			flxFiles.Rows.Clear();

			try
			{
				string pathStr = txtPath.Text;

				if (!CheckPath(pathStr))
				{
					throw new Exception("路径不合法");
				}

				DirectoryInfo dir = new DirectoryInfo(pathStr);

				// 分4步保存到数据库
				m_FolderHelper.ClearDBList();					// 先清空列表
				m_FolderHelper.AddFileToList(dir, null);		// 本目录下的文件
				m_FolderHelper.AddDirToList(dir, null);		// 本目录下的子目录(包括文件)
				m_FolderHelper.SavaData();					// 保存到数据库
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);

				txtPath.Text = "";
				txtPath.Focus();
			}
			finally
			{
				SetEnabled(false);
			}
		}

		/// <summary>
		/// 检查路径是否以":"或":\"或":/"结尾
		/// </summary>
		/// <param name="path">路径</param>
		/// <returns></returns>
		private bool CheckPath(string path)
		{
			if (path.EndsWith(":") || path.EndsWith(":\\") || path.EndsWith(":/"))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 设置txt_Search、btn_Search、ddlst_Extension的enabled
		/// </summary>
		/// <param name="enable"></param>
		private void SetEnabled(bool enable)
		{
			txtKeyword.IsEnabled = enable;
			btnSearch.IsEnabled = enable;
			ddlstExtension.IsEnabled = enable;
		}

		private void ddlstExtension_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ddlstExtension.SelectedIndex >= 0)
			{
				flxFiles.ItemsSource = null;

				// 临时集合,默认存放所有
				List<List<string>> files = m_FolderHelper.FileList;

				// 选择后缀
				if (ddlstExtension.SelectedIndex > 0)
				{
					files = new List<List<string>>();

					// 选择的后缀名
					string extension = ddlstExtension.SelectedValue.ToString();

					// 筛选
					foreach (List<string> file in m_FolderHelper.FileList)
					{
						if (extension == file[2])	// 后缀
						{
							files.Add(file);
						}
					}
				}

				flxFiles.ItemsSource = files;
			}

		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			List<List<string>> files = new List<List<string>>();

			string keyword = txtKeyword.Text.ToLower();		// 忽略大小写

			if (keyword != string.Empty)
			{
				flxFiles.ItemsSource = null;

				foreach (List<string> file in m_FolderHelper.FileList)
				{
					// file[0]为名称
					if (file[0].ToLower().Contains(keyword))
					{
						files.Add(file);
					}
				}

				flxFiles.ItemsSource = files;

				ddlstExtension.SelectedIndex = -1;
			}
			else
			{
				ddlstExtension.SelectedIndex = 0;
			}
		}

		private void txtKeyword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btnSearch_Click(null, null);
			}
		}

		private void txtKeyword_TextChanged(object sender, TextChangedEventArgs e)
		{
			btnSearch_Click(null, null);
		}

		private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
			// 清空列表
			flxFolder.ItemsSource = null;
			flxFiles.ItemsSource = null;

			m_FolderHelper.GetAllFolders();		// 从目录中读取

			m_FolderHelper.onLoadDirFinish += new EventHandler(LoadDirFinish);	// 订阅事件
		}

		/// <summary>
		/// 读取完成，构造树，填充flexgrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadDirFinish(object sender, EventArgs e)
		{
			GroupRow newRow = new GroupRow();
			flxFolder.Rows.Add(newRow);
			// 显示的是名字
			newRow[0] = m_FolderHelper.DirList[0][2];
			// 后台存的是ID
			newRow.Tag = Convert.ToInt32(m_FolderHelper.DirList[0][0]);
			newRow.Level = 0;

			// 用于存放PID为NULL的目录，相当于几棵目录树的根节点，所以叫treeRoots
			List<List<string>> treeRoots = new List<List<string>>();

			// 找PID为NULL的目录
			foreach (var dir in m_FolderHelper.DirList)
			{
				if (dir[1] == "NULL")
				{
					treeRoots.Add(dir);
				}
			}

			// 先序遍历各棵目录树
			foreach (var treeRoot in treeRoots)
			{
				BindTreeNode(treeRoot, 1);
			}
		}

		/// <summary>
		/// 构建树
		/// </summary>
		/// <param name="dir">目录相关信息</param>
		/// <param name="level">深度</param>
		private void BindTreeNode(List<string> dir, int level)
		{
			GroupRow newRow = new GroupRow();
			flxFolder.Rows.Add(newRow);
			newRow[0] = dir[2];						// 名称
			newRow.Tag = Convert.ToInt32(dir[0]);	// ID
			newRow.Level = level;

			foreach (List<string> tempDir in m_FolderHelper.DirList)
			{
				// 找“孩子”
				// tempDir的PID等于dir的ID
				if (tempDir[1] == dir[0])
				{
					BindTreeNode(tempDir, level + 1);
				}
			}
		}

		/// <summary>
		/// 读取完成，绑定数据源，添加后缀到选择后缀下拉框
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadFileFinish(object sender, EventArgs e)
		{
			m_Extensions.Clear();
			m_Extensions.Add("ALL");

			foreach (List<string> file in m_FolderHelper.FileList)
			{
				string fileExt = file[2];			// 后缀

				// 如果此后缀下拉框中不存在，就加进去
				if (!m_Extensions.Contains(fileExt))
				{
					m_Extensions.Add(fileExt);
				}
			}

			flxFiles.ItemsSource = m_FolderHelper.FileList;

			SetEnabled(true);
		}

		/// <summary>
		/// 创建行序号方法
		/// </summary>
		/// <param name="flexGrid">界面FlexGrid实例</param>
		/// <param name="border">单元格根控件</param>
		/// <param name="cellRange">选择范围</param>
		/// <returns>是否使用默认生成方法</returns>
		private bool CreateRowHeader(C1FlexGrid flexGrid, Border border, CellRange cellRange)
		{
			string text = (cellRange.Row + 1).ToString();

			TextBlock textBlock = new TextBlock();
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			textBlock.Text = text;
			border.Child = textBlock;

			return false;
		}

		private void fgFolder_SelectionChanged(object sender, CellRangeEventArgs e)
		{
			int index = e.Row;		// 选择的行的index

			if (index >= 0)
			{
				SetEnabled(false);

				int id = (int) flxFolder.Rows[index].Tag;

				m_FolderHelper.GetFiles(id);

				// 订阅事件
				m_FolderHelper.onLoadFileFinish -= new EventHandler(LoadFileFinish);
				m_FolderHelper.onLoadFileFinish += new EventHandler(LoadFileFinish);

				flxFiles.ItemsSource = null;
			}
		}
	}
}
