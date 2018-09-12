using System;
using System.Text;

namespace YW.Manage.BLL
{
    public class PagingExtensions
    {
        /// <summary>
        /// Url分页
        /// </summary>
        /// <param name="currentPage">当前页面</param>
        /// <param name="currentPageSize">每页数量</param>
        /// <param name="totalRecords">数据全部数量</param>
        /// <param name="urlPrefix">地址</param>
        /// <returns></returns>
        public static string Pager(int currentPage, int currentPageSize, int totalRecords, string urlPrefix)
        {
            StringBuilder sb1 = new StringBuilder();
            int pageCount = totalRecords / currentPageSize;
            if (totalRecords % currentPageSize > 0)
            {
                pageCount = pageCount + 1;
            }
            //显示数量
            int pageCountNumber = 10;

            //获取分页数值,当当前页大于10时获取基数
            int count = currentPage > 10 ? currentPage / 10 : 0;

            //起始数
            int seed = pageCount > 10 ? 10 * count : 0;
            string linStr = "";
            if (urlPrefix.IndexOf('?') > 0)
            {
                linStr = "&";
            }
            else
            {
                linStr = "?";
            }
            urlPrefix = urlPrefix + linStr;

            if (pageCount > 1)
            {
                sb1.Append("<div class=\"pager float_right\">");
                if (currentPage > 1)
                {
                    sb1.AppendLine(String.Format("<a href=\"{0}page={1}\" class=\"page-f\"></a>", urlPrefix, currentPage - 1));
                }
                else
                {
                    sb1.AppendLine(String.Format("<span class=\"page-f-none\"></span>"));
                }

                if (currentPage > pageCountNumber)
                {
                    sb1.AppendLine(String.Format("<a href=\"{0}page={1}\" class=\"page\") >...</a>", urlPrefix, (seed - pageCountNumber) + 1));
                }
                for (int i = seed; i < pageCount && i < seed + pageCountNumber; i++)
                {
                    if (currentPage == i + 1)
                    {
                        sb1.AppendLine(String.Format("<a href=\"{0}page={1}\" class=\"page sel\">{1}</a>", urlPrefix, i + 1));
                    }
                    else
                    {
                        sb1.AppendLine(String.Format("<a href=\"{0}page={1}\" class=\"page\">{1}</a>", urlPrefix, i + 1));
                    }
                }

                if (count < pageCount / 10)
                {
                    sb1.AppendLine(String.Format("<a href=\"{0}page={1}\" class=\"page\">...</a>", urlPrefix, (seed + pageCountNumber) + 1));
                }


                if (currentPage < pageCount)
                {
                    sb1.AppendLine(String.Format("<a href=\"{0}page={1}\" class=\"page-b\"></a>", urlPrefix, currentPage + 1));
                }
                else
                {
                    sb1.AppendLine(String.Format("<span class=\"page-b-none\"></span>"));
                }

                sb1.Append("</div>");
            }
            return sb1.ToString();
        }


        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="currentPage">当前页数</param>
        /// <param name="currentPageSize">当前页显示数量</param>
        /// <param name="totalRecords">数据全部数量</param>
        /// <param name="urlPrefix">链接地址</param>
        /// <returns></returns>
        public static string PagerAjax(int currentPage, int currentPageSize, int totalRecords, string urlPrefix)
        {
            StringBuilder sb1 = new StringBuilder();
            int pageCount = totalRecords / currentPageSize;
            if (totalRecords % currentPageSize > 0)
            {
                pageCount = pageCount + 1;
            }

            //显示数量
            int pageCountNumber = 10;

            //获取分页数值,当当前页大于10时获取基数
            int count = currentPage > 10 ? currentPage / 10 : 0;

            //起始数
            int seed = pageCount > 10 ? 10 * count : 0;

            string linStr = "";
            if (urlPrefix.IndexOf('?') > 0)
            {
                linStr = "&";
            }
            else
            {
                linStr = "?";
            }
            urlPrefix = urlPrefix + linStr;
            if (pageCount > 1)
            {
                sb1.Append("<div class=\"pager float_right\">");
                if (currentPage > 1)
                {
                    sb1.AppendLine(String.Format("<a href=\"javascript:void(0);\" onclick=\"AjaxPageTable('{0}',{1},'previous');\" class=\"page-f\"></a>", urlPrefix, currentPage - 1));
                }
                else
                {
                    sb1.AppendLine(String.Format("<span class=\"page-f-none\"></span>"));
                }

                if (currentPage > pageCountNumber)
                {
                    sb1.AppendLine(String.Format("<a href=\"javascript:void(0);\" onclick=\"AjaxPageTable('{0}',{1},'previous');\" class=\"page\") >...</a>", urlPrefix, (seed - pageCountNumber) + 1));
                }
                for (int i = seed; i < pageCount && i < seed + pageCountNumber; i++)
                {
                    if (currentPage == i + 1)
                    {
                        sb1.AppendLine(String.Format("<a  href=\"javascript:void(0);\" onclick=\"AjaxPageTable('{0}',{1},'page');\" class=\"page sel\">{1}</a>", urlPrefix, i + 1));
                    }
                    else
                    {
                        sb1.AppendLine(String.Format("<a  href=\"javascript:void(0);\" onclick=\"AjaxPageTable('{0}',{1},'page');\" class=\"page\">{1}</a>", urlPrefix, i + 1));
                    }
                }

                if (count < pageCount / 10)
                {
                    sb1.AppendLine(String.Format("<a href=\"javascript:void(0);\" onclick=\"AjaxPageTable('{0}',{1},'previous');\" class=\"page\">...</a>", urlPrefix, (seed + pageCountNumber) + 1));
                }

                if (currentPage < pageCount)
                {

                    sb1.AppendLine(String.Format("<a href=\"javascript:void(0);\" onclick=\"AjaxPageTable('{0}',{1},'next');\" class=\"page-b next\"></a>", urlPrefix, currentPage + 1));
                }
                else
                {
                    sb1.AppendLine(String.Format("<span class=\"page-b-none\"></span>"));
                }
                sb1.Append("</div>");
            }

            return sb1.ToString();
        }

    }
}