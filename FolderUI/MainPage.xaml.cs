using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
			ddlst_Extension.ItemsSource = m_Extensions;
			m_Extensions.Add("ALL");
		}

		private void btn_Save_Click(object sender, RoutedEventArgs e)
		{
			tree_Folder.ItemsSource = null;
			dg_File.ItemsSource = null;

			try
			{
				string pathStr = txt_Path.Text;

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

				txt_Path.Text = "";
				txt_Path.Focus();
				SetEnabled(false);
				return;
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
			txt_Search.IsEnabled = enable;
			btn_Search.IsEnabled = enable;
			ddlst_Extension.IsEnabled = enable;
		}

		private void ddlst_Extension_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// 临时集合，默认显示“ALL”
			List<List<string>> files = m_FolderHelper.FileList;

			// 选择后缀
			if (ddlst_Extension.SelectedIndex > 0)
			{
				files = new List<List<string>>();

				// 选择的后缀名
				string extension = ddlst_Extension.SelectedValue.ToString();

				// 筛选
				foreach (var file in m_FolderHelper.FileList)
				{
					if (file[3] == extension)
					{
						files.Add(file);
					}
				}
			}

			dg_File.ItemsSource = files;
		}

		private void btn_Search_Click(object sender, RoutedEventArgs e)
		{
			string keyword = txt_Search.Text.ToLower();		// 忽略大小写

			if (keyword != string.Empty)
			{
				List<List<string>> files = new List<List<string>>();

				foreach (var file in m_FolderHelper.FileList)
				{
					if (file[0].ToLower().Contains(keyword))
					{
						files.Add(file);
					}
				}

				dg_File.ItemsSource = files;
			}
			else
			{
				ddlst_Extension.SelectedIndex = 0;
			}
		}

		private void txt_Search_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btn_Search_Click(null, null);
			}
		}

		private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
		{
			btn_Search_Click(null, null);
		}

		private void btn_Load_Click(object sender, RoutedEventArgs e)
		{
			m_FolderHelper.GetAllFolders();		// 从目录中读取

			m_FolderHelper.onLoadDirFinish += new EventHandler(LoadDirFinish);	// 订阅事件
		}

		/// <summary>
		/// 读取完成，绑定数据源
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadDirFinish(object sender, EventArgs e)
		{
			tree_Folder.ItemsSource = m_FolderHelper.DirTree;
			SetEnabled(true);
		}

		/// <summary>
		/// 读取完成，绑定数据源，添加后缀到选择后缀下拉框
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadFileFinish(object sender, EventArgs e)
		{
			dg_File.ItemsSource = m_FolderHelper.FileList;

			m_Extensions.Clear();
			m_Extensions.Add("ALL");

			foreach (List<string> file in m_FolderHelper.FileList)
			{
				string fileExt = file[2];		// 后缀

				if (!m_Extensions.Contains(fileExt))
				{
					m_Extensions.Add(fileExt);
				}
			}
		}

		private void tree_Folder_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeViewItem nowNode = (TreeViewItem) tree_Folder.SelectedItem;

			if (nowNode != null)
			{
				m_FolderHelper.GetFiles((int) nowNode.Tag);

				// 订阅事件
				m_FolderHelper.onLoadFileFinish -= new EventHandler(LoadFileFinish);
				m_FolderHelper.onLoadFileFinish += new EventHandler(LoadFileFinish);
			}
		}
	}
}
