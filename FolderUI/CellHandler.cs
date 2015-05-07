using System.Windows;
using System.Windows.Controls;

using C1.Silverlight.FlexGrid;

namespace HuaweiSoftware.ZJNET.CommonSL
{
    /// <summary>
    /// 重写cell的委托
    /// </summary>
    /// <param name="flexGrid">需要处理的FlexGrid</param>
    /// <param name="border">单元格的容器</param>
    /// <param name="cellRange">单元格的位置</param>
    /// <returns>是否需要调用父类的重写方法</returns>
    public delegate bool NewCellAction(C1FlexGrid flexGrid, Border border, CellRange cellRange);

    public class CellHandler : CellFactory
    {
        /// <summary>
        /// 重新内容
        /// </summary>
        public NewCellAction MyCell;

        /// <summary>
        /// 重写列头
        /// </summary>
        public NewCellAction MyHeader;

        /// <summary>
        /// 重写行标题
        /// </summary>
        public NewCellAction MyRowHeader;

        /// <summary>
        /// 重写内容中cell
        /// </summary>
        /// <param name="flexGrid">需要处理的FlexGrid</param>
        /// <param name="border">Cell的容器</param>
        /// <param name="range">Cell的位置信息</param>
        public override void CreateCellContent(C1FlexGrid flexGrid, Border border, CellRange range)
        {
            if (MyCell == null)
            {          
                base.CreateCellContent(flexGrid, border, range);
            }
            else if (MyCell(flexGrid, border, range)) // 调用委托的方法
            {
                base.CreateCellContent(flexGrid, border, range);
            }
        }

        /// <summary>
        /// 重写FlexGrid的列头
        /// </summary>
        /// <param name="flexGrid">需要处理的FlexGrid</param>
        /// <param name="border">列头的容器</param>
        /// <param name="range">列头的位置信息</param>
        public override void CreateColumnHeaderContent(C1FlexGrid flexGrid, Border border, CellRange range)
        {
            if (MyHeader == null)
            {
                base.CreateColumnHeaderContent(flexGrid, border, range);
            }
            else if (MyHeader(flexGrid, border, range)) // 调用委托的方法
            {
                base.CreateColumnHeaderContent(flexGrid, border, range);
            }
        }

        /// <summary>
        /// 重写FlexGrid的行标题
        /// </summary>
        /// <param name="flexGrid">需要处理的FlexGrid</param>
        /// <param name="border">列头的容器</param>
        /// <param name="range">列头的位置信息</param>
        public override void CreateRowHeaderContent(C1FlexGrid flexGrid, Border border, CellRange range)
        {
            if (MyRowHeader == null)
            {
                base.CreateRowHeaderContent(flexGrid, border, range);
            }
            else if (MyRowHeader(flexGrid, border, range)) // 调用委托的方法
            {
                base.CreateRowHeaderContent(flexGrid, border, range);
            }
        }
    }

    /// <summary>
    /// 重写cell的委托
    /// </summary>
    /// <param name="grid">需要处理的FlexGrid</param>
    /// <param name="cellType">单元格的类型</param>
    /// <param name="range">单元格的位置</param>
    public delegate void CellTipAction(FrameworkElement cell, CellType cellType, CellRange cellRange);

    public class CellTipHandler : CellHandler
    {
        /// <summary>
        /// 重写ToolTip
        /// </summary>
        public CellTipAction CellTip;

        /// <summary>
        /// 增加ToolTip
        /// </summary>
        /// <param name="flexGrid">需要处理的FlexGrid</param>
        /// <param name="cellType">单元格类型</param>
        /// <param name="range">单元格位置</param>
        /// <returns>单元格对象</returns>
        public override FrameworkElement CreateCell(C1FlexGrid flexGrid, CellType cellType, CellRange range)
        {
            FrameworkElement cell = base.CreateCell(flexGrid, cellType, range);
            if (CellTip != null)
            {
                CellTip(cell, cellType, range);
            }

            return cell;
        }
    }
}
