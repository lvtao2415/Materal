﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Materal.WPFCustomControlLib.SearchBox
{
    public class SearchBox : ComboBox
    {
        /// <summary>
        /// 查询方法
        /// </summary>
        public Func<object, bool> SearchFun { get; set; } = m => true;
        /// <summary>
        /// 选中方法
        /// </summary>
        public Func<object, bool> SelectedFun { get; set; } = m => true;
        /// <summary>
        /// 显示最大数量(-1为不限制)
        /// </summary>
        public int ShowMaxNum { get => (int)GetValue(ShowMaxNumProperty); set => SetValue(ShowMaxNumProperty, value); }
        public static readonly DependencyProperty ShowMaxNumProperty = DependencyProperty.Register(nameof(ShowMaxNum), typeof(int), typeof(SearchBox),
            new FrameworkPropertyMetadata(
                -1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

        /// <summary>
        /// 候选数据
        /// </summary>
        public IEnumerable<object> CandidateData { get => (IEnumerable<object>)GetValue(CandidateDataProperty); set => SetValue(CandidateDataProperty, value); }
        public static readonly DependencyProperty CandidateDataProperty = DependencyProperty.Register(nameof(CandidateData), typeof(IEnumerable<object>), typeof(SearchBox),
            new FrameworkPropertyMetadata(
                new List<object>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnCandidateDataChanged,
                CoerceCandidateData));
        private static void OnCandidateDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is SearchBox searchBox)) return;
            searchBox.ItemsSource = searchBox.CandidateShowData;
        }
        private static object CoerceCandidateData(DependencyObject sender, object value)
        {
            if (!(sender is SearchBox searchBox)) return value;
            if (!(value is IEnumerable<object> values)) return value;
            IEnumerable<object> data = values.ToList().Where(searchBox.SearchFun);
            List<object> result = searchBox.ShowMaxNum < 0 ? data.ToList() : data.Take(searchBox.ShowMaxNum).ToList();
            searchBox.CandidateShowData.Clear();
            foreach (object item in result)
            {
                searchBox.CandidateShowData.Add(item);
            }
            return value;
        }
        /// <summary>
        /// 候选项显示数据
        /// </summary>
        public ObservableCollection<object> CandidateShowData { get; set; } = new ObservableCollection<object>();
        /// <summary>
        /// 应用模版
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_EditableTextBox") is TextBox textBox) textBox.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateShowData();
            IsDropDownOpen = CandidateShowData.Count > 0;
            if (sender is TextBox textBox)
            {
                textBox.SelectionStart = Text.Length;
            }
        }
        /// <summary>
        /// 更新显示数据
        /// </summary>
        public void UpdateShowData()
        {
            object[] data = ShowMaxNum < 0 ? CandidateData.Where(SearchFun).ToArray() : CandidateData.Where(SearchFun).Take(ShowMaxNum).ToArray();
            CandidateShowData.Clear();
            foreach (object item in data)
            {
                CandidateShowData.Add(item);
            }
            object obj = CandidateData.FirstOrDefault(SelectedFun);
            if (obj != null) SelectedItem = obj;
        }

        /// <inheritdoc />
        /// <summary>
        /// 构造方法
        /// </summary>
        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }
    }
}
