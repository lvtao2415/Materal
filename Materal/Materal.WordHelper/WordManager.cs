﻿using Materal.Common;
using Materal.WordHelper.Model;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Materal.StringHelper;

namespace Materal.WordHelper
{
    public class WordManager
    {
        #region 读取
        /// <summary>
        /// 读取文档
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public XWPFDocument ReadWord(string filePath)
        {
            if (!File.Exists(filePath)) throw new MateralException("文件不存在");
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return ReadWord(fileStream);
            }
        }
        /// <summary>
        /// 读取文档
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public XWPFDocument ReadWord(FileStream fileStream)
        {
            return new XWPFDocument(fileStream);
        }
        #endregion
        /// <summary>
        /// 应用模板
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <param name="templates">模板模型</param>
        public void ApplyToTemplate(XWPFDocument document, params TemplateModel[] templates)
        {
            var stringTemplates = new List<StringTemplateModel>();
            var tableTemplates = new List<TableTemplateModel>();
            foreach (TemplateModel template in templates)
            {
                switch (template)
                {
                    case StringTemplateModel stringTemplate:
                        stringTemplates.Add(stringTemplate);
                        break;
                    case TableTemplateModel tableTemplate:
                        tableTemplates.Add(tableTemplate);
                        break;
                }
            }
            if (stringTemplates.Count > 0)
            {
                ApplyToStringTemplate(document, stringTemplates);
            }
            if (tableTemplates.Count > 0)
            {
                ApplyToTableTemplate(document, tableTemplates, stringTemplates);
            }
        }
        /// <summary>
        /// 应用字符串模板
        /// </summary>
        /// <param name="document"></param>
        /// <param name="stringTemplates"></param>
        private void ApplyToStringTemplate(IBody document, IReadOnlyCollection<StringTemplateModel> stringTemplates)
        {
            foreach (XWPFParagraph paragraph in document.Paragraphs)
            {
                string oldText = paragraph.ParagraphText;
                if (string.IsNullOrEmpty(oldText)) continue;
                string newText = oldText;
                foreach (StringTemplateModel template in stringTemplates)
                {
                    string key = "{$" + template.Key + "}";
                    if (!newText.Contains(key)) continue;
                    newText = newText.Replace(key, template.Value);
                }
                if (newText != oldText)
                {
                    paragraph.ReplaceText(oldText, newText);
                }
            }
        }
        /// <summary>
        /// 应用表格模板
        /// </summary>
        /// <param name="document"></param>
        /// <param name="tableTemplates"></param>
        /// <param name="stringTemplates"></param>
        private void ApplyToTableTemplate(IBody document, IReadOnlyCollection<TableTemplateModel> tableTemplates, IReadOnlyCollection<StringTemplateModel> stringTemplates)
        {
            foreach (XWPFTable tableContent in document.Tables)
            {
                var isApply = false;
                for (var rowIndex = 0; rowIndex < tableContent.NumberOfRows; rowIndex++)
                {
                    (string tableName, List<KeyValuePair<int, string>> colNames) = GetTableNameAndColName(tableContent, rowIndex);
                    if (string.IsNullOrEmpty(tableName)) continue;
                    TableTemplateModel tableTemplate = tableTemplates.FirstOrDefault(m => m.Key == tableName);
                    if (tableTemplate == null) continue;
                    ApplyToTableTemplate(tableContent, tableTemplate, colNames);
                    isApply = true;
                }
                if (!isApply)
                {
                    ApplyTableToStringTemplate(tableContent, stringTemplates);
                }
            }
        }
        /// <summary>
        /// 应用表格模板
        /// </summary>
        /// <param name="tableContent"></param>
        /// <param name="tableTemplate"></param>
        /// <param name="colNames"></param>
        private void ApplyToTableTemplate(XWPFTable tableContent, TableTemplateModel tableTemplate, IReadOnlyCollection<KeyValuePair<int, string>> colNames)
        {
            int startRowNum = tableTemplate.StartRowNumber;
            int rowNumber = tableTemplate.Value.Rows.Count;
            for (var rowIndex = 0; rowIndex < rowNumber; rowIndex++)
            {
                int documentRowIndex = rowIndex + startRowNum;
                XWPFTableRow row = tableContent.NumberOfRows <= documentRowIndex ? tableContent.CreateRow() : tableContent.GetRow(documentRowIndex);
                int cellCount = row.GetTableCells().Count;
                for (int i = cellCount - 1; i < colNames.Max(m => m.Key); i++)
                {
                    row.CreateCell();
                }
                foreach (KeyValuePair<int, string> colName in colNames)
                {
                    string value = tableTemplate.Value.Rows[rowIndex][colName.Value].ToString();
                    XWPFParagraph paragraph = GetCellContent(rowIndex, colName.Key, tableContent, value, tableTemplate.OnSetCellText);
                    row.GetCell(colName.Key).SetParagraph(paragraph);
                }
            }
        }
        /// <summary>
        /// 应用表格文本模板
        /// </summary>
        /// <param name="tableContent"></param>
        /// <param name="stringTemplates"></param>
        private void ApplyTableToStringTemplate(XWPFTable tableContent, IReadOnlyCollection<StringTemplateModel> stringTemplates)
        {
            foreach (XWPFTableRow row in tableContent.Rows)
            {
                foreach (XWPFTableCell cell in row.GetTableCells())
                {
                    IBodyElement bodyElement = cell.BodyElements[0];
                    if (!(bodyElement is XWPFParagraph paragraph)) continue;
                    string oldText = paragraph.ParagraphText;
                    if (string.IsNullOrEmpty(oldText)) continue;
                    string newText = oldText;
                    foreach (StringTemplateModel template in stringTemplates)
                    {
                        string key = "{$" + template.Key + "}";
                        if (!newText.Contains(key)) continue;
                        newText = newText.Replace(key, template.Value);
                    }
                    if (newText != oldText)
                    {
                        paragraph.ReplaceText(oldText, newText);
                    }
                }
            }
        }
        /// <summary>
        /// 获得表名和列表
        /// </summary>
        /// <param name="tableContent"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private (string tableName, List<KeyValuePair<int, string>> colNames) GetTableNameAndColName(XWPFTable tableContent, int rowIndex)
        {
            XWPFTableRow row = tableContent.GetRow(rowIndex);
            List<XWPFTableCell> cells = row.GetTableCells();
            if (cells.Count <= 0 || cells[0].BodyElements.Count <= 0) return (null, null);
            string tableName = string.Empty;
            var colNames = new List<KeyValuePair<int, string>>();
            for (var index = 0; index < cells.Count; index++)
            {
                IBodyElement bodyElement = cells[index].BodyElements[0];
                if (!(bodyElement is XWPFParagraph paragraph)) continue;
                string value = paragraph.ParagraphText;
                if (!value.VerifyRegex("^{\\$.+\\..+\\}$")) continue;
                string[] tableNameAndColumnName = value.Substring(2, value.Length - 3).Split('.');
                if (tableNameAndColumnName.Length != 2) continue;
                tableName = tableNameAndColumnName[0];
                colNames.Add(new KeyValuePair<int, string>(index, tableNameAndColumnName[1]));
            }

            return (tableName, colNames);
        }
        /// <summary>
        /// 获取单元格内容
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="colIndex"></param>
        /// <param name="tableContent"></param>
        /// <param name="value"></param>
        /// <param name="setCell"></param>
        /// <returns></returns>
        private XWPFParagraph GetCellContent(int rowIndex, int colIndex, IBodyElement tableContent, string value, Func<int, int, XWPFParagraph, XWPFRun> setCell)
        {
            var ctP = new CT_P();
            var paragraph = new XWPFParagraph(ctP, tableContent.Body);
            if (setCell == null)
            {
                paragraph.VerticalAlignment = TextAlignment.CENTER;
                paragraph.Alignment = ParagraphAlignment.CENTER;
                XWPFRun run = paragraph.CreateRun();
                run.SetText(value);
            }
            else
            {
                setCell.Invoke(rowIndex, colIndex, paragraph);
            }
            return paragraph;
        }
    }
}