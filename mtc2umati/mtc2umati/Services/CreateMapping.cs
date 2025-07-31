// SPDX-License-Identifier: Apache-2.0
// Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

using System.Data;
using ClosedXML.Excel;

namespace mtc2umati.Services
{

    public class MappedObject(string ModellingRule, string opcPath, string opcDataType,
        string mtcName, string mtcPath, string mtcDataType, string mtcSubtype, object? value = null)
    {
        public string ModellingRule { get; set; } = ModellingRule;
        public string OpcPath { get; set; } = opcPath;
        public string OpcDataType { get; set; } = opcDataType;
        public string MtcName { get; set; } = mtcName;
        public string MtcPath { get; set; } = mtcPath;
        public string MtcDataType { get; set; } = mtcDataType;
        public string MtcSubtype { get; set; } = mtcSubtype;
        public object? Value { get; set; } = value;
        public object? ConvertedValue { get; set; } = null;
    }

    public static class MappingLoader
    {
        public static List<MappedObject> LoadMappingFromDataTable(DataTable dataTable)
        {
            var mappedObjects = new List<MappedObject>();

            foreach (DataRow row in dataTable.Rows)
            {
                var mappedObject = new MappedObject(
                    row["Modelling Rule"]?.ToString()?.Trim() ?? string.Empty,
                    row["OPC Path"]?.ToString()?.Trim() ?? string.Empty,
                    row["Data Type"]?.ToString()?.Trim() ?? string.Empty,
                    row["MTC Name"]?.ToString()?.Trim() ?? string.Empty,
                    row["MTC Path"]?.ToString()?.Trim() ?? string.Empty,
                    row["MTC Data Type"]?.ToString()?.Trim() ?? string.Empty,
                    row["subType"]?.ToString()?.Trim() ?? string.Empty
                );
                mappedObjects.Add(mappedObject);
            }

            Console.WriteLine($"[INFO] {mappedObjects.Count} mapped objects loaded from DataTable.");

            return mappedObjects;
        }

        public static List<MappedObject>? LoadMapping(string filePath, string sheetName)
        {
            // Define the columns needed for mapping
            var columnsToRead = new[] { "Modelling Rule", "OPC Path", "Data Type", "MTC Name", "MTC Path", "subType", "MTC Data Type" };
            var columnsToIgnore = new[] { "subType" };
            var dataTable = DataTableExtensions.ReadMappingXlsxFile(filePath, sheetName, columnsToRead);

            if (dataTable == null)
            {
                Console.WriteLine("[ERROR] Cannot load DataTable.");
                return null;
            }

            dataTable.RemoveIncompleteRows(columnsToIgnore);

            var mappedObjects = LoadMappingFromDataTable(dataTable);

            return mappedObjects;
        }

        public static void ShowMappedObjects(this List<MappedObject> mappedObjects)
        {
            Console.WriteLine("--- Mapped Values ---");
            foreach (var obj in mappedObjects)
            {
                Console.WriteLine($"Modelling Rule: {obj.ModellingRule}");
                Console.WriteLine($"OPC Path: {obj.OpcPath}");
                Console.WriteLine($"MTC Path: {obj.MtcPath}");
                if (!string.IsNullOrEmpty(obj.MtcSubtype))
                {
                    Console.WriteLine($"MTC Subtype: {obj.MtcSubtype}");
                }
                Console.WriteLine($"Value: {obj.Value}\n");
            }
        }
    }

    public static class DataTableExtensions
    {
        public static DataTable? ReadMappingXlsxFile(string filePath, string sheetName, string[] columnsToRead)
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(sheetName);
            if (worksheet == null)
            {
                Console.WriteLine($"Sheet '{sheetName}' not found!");
                return null;
            }

            var headerRow = worksheet.Row(1);
            var columnIndices = headerRow.CellsUsed()
                .Where(cell => columnsToRead.Contains(cell.GetString()))
                .ToDictionary(cell => cell.GetString(), cell => cell.Address.ColumnNumber);

            if (columnsToRead.Any(col => !columnIndices.ContainsKey(col)))
            {
                Console.WriteLine("At least one row could not be found!");
                return null;
            }

            var dataTable = new DataTable();

            foreach (var col in columnsToRead)
                dataTable.Columns.Add(col, typeof(string));

            var dataRows = worksheet.RowsUsed().Where(r => r.RowNumber() > 2);

            foreach (var row in dataRows)
            {
                var values = columnsToRead
                    .Select(colName => row.Cell(columnIndices[colName]).GetValue<string>())
                    .ToArray();

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static void ShowDataTable(this DataTable table)
        {
            foreach (DataColumn column in table.Columns)
                Console.Write($"{column.ColumnName}\t");

            Console.WriteLine();

            foreach (DataRow row in table.Rows)
            {
                foreach (var item in row.ItemArray)
                    Console.Write($"{item}\t");

                Console.WriteLine();
            }
        }

        public static void RemoveIncompleteRows(this DataTable table, params string[] columnsToIgnore)
        {
            var columnsToCheck = table.Columns.Cast<DataColumn>()
                                    .Where(col => !columnsToIgnore.Contains(col.ColumnName))
                                    .ToList();

            var rowsToDelete = table.AsEnumerable()
                                    .Where(row => columnsToCheck.Any(col => string.IsNullOrWhiteSpace(row[col].ToString())))
                                    .ToList();

            foreach (var row in rowsToDelete)
                table.Rows.Remove(row);
        }
    }
}