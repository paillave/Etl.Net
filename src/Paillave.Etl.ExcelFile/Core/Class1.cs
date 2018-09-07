//public static void RunSample13()
//{
//    var pck = new ExcelPackage();

//    //Create a datatable with the directories and files from the root directory...
//    DataTable dt = GetDataTable(Utils.GetDirectoryInfo("."));

//    var wsDt = pck.Workbook.Worksheets.Add("FromDataTable");

//    //Load the datatable and set the number formats...
//    wsDt.Cells["A1"].LoadFromDataTable(dt, true, TableStyles.Medium9);
//    wsDt.Cells[2, 2, dt.Rows.Count + 1, 2].Style.Numberformat.Format = "#,##0";
//    wsDt.Cells[2, 3, dt.Rows.Count + 1, 4].Style.Numberformat.Format = "mm-dd-yy";
//    wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
//    //Select Name and Created-time...
//    var collection = (from row in dt.Select() select new { Name = row["Name"], Created_time = (DateTime)row["Created"] });

//    var wsEnum = pck.Workbook.Worksheets.Add("FromAnonymous");

//    //Load the collection starting from cell A1...
//    wsEnum.Cells["A1"].LoadFromCollection(collection, true, TableStyles.Medium9);

//    //Add some formating...
//    wsEnum.Cells[2, 2, dt.Rows.Count - 1, 2].Style.Numberformat.Format = "mm-dd-yy";
//    wsEnum.Cells[wsEnum.Dimension.Address].AutoFitColumns();

//    //Load a list of FileDTO objects from the datatable...
//    var wsList = pck.Workbook.Worksheets.Add("FromList");
//    List<FileDTO> list = (from row in dt.Select()
//                          select new FileDTO
//                          {
//                              Name = row["Name"].ToString(),
//                              Size = row["Size"].GetType() == typeof(long) ? (long)row["Size"] : 0,
//                              Created = (DateTime)row["Created"],
//                              LastModified = (DateTime)row["Modified"],
//                              IsDirectory = (row["Size"] == DBNull.Value)
//                          }).ToList<FileDTO>();

//    //Load files ordered by size...
//    wsList.Cells["A1"].LoadFromCollection(from file in list
//                                          orderby file.Size descending
//                                          where file.IsDirectory == false
//                                          select file, true, TableStyles.Medium9);

//    wsList.Cells[2, 2, dt.Rows.Count + 1, 2].Style.Numberformat.Format = "#,##0";
//    wsList.Cells[2, 3, dt.Rows.Count + 1, 4].Style.Numberformat.Format = "mm-dd-yy";


//    //Load directories ordered by Name...
//    var r1 = wsList.Cells["F1"].LoadFromCollection(from file in list
//                                                   orderby file.Name ascending
//                                                   where file.IsDirectory == true
//                                                   select new
//                                                   {
//                                                       Name = file.Name,
//                                                       Created = file.Created,
//                                                       Last_modified = file.LastModified
//                                                   }, //Use an underscore in the property name to get a space in the title.
//                                          true, TableStyles.Medium11);

//    wsList.Cells[2, 7, dt.Rows.Count + 1, 8].Style.Numberformat.Format = "mm-dd-yy";

//    //Load the list using a specified array of MemberInfo objects. Properties, fields and methods are supported.
//    var rng = wsList.Cells["J1"].LoadFromCollection(list,
//                                          true,
//                                          TableStyles.Medium10,
//                                          BindingFlags.Instance | BindingFlags.Public,
//                                          new MemberInfo[] {
//                                                      typeof(FileDTO).GetProperty("Name"),
//                                                      typeof(FileDTO).GetField("IsDirectory"),
//                                                      typeof(FileDTO).GetMethod("ToString")}
//                                          );

//    wsList.Tables.GetFromRange(rng).Columns[2].Name = "Description";

//    wsList.Cells[wsList.Dimension.Address].AutoFitColumns();

//    //...and save
//    var fi = Utils.GetFileInfo("sample13.xlsx");
//    pck.SaveAs(fi);
//}
