using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.DFZY.RuiDao.CusTrustedBalanceValiPlugin
{
    [Description("销售充值单校验可信额度")]
    [HotUpdate]
    public class CusTrustedBalanceValiPlugin : AbstractOperationServicePlugIn
    {
        //OnPreparePropertys 数据加载前，确保需要的属性被加载
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillHead");
            e.FieldKeys.Add("F_SRT_CONTRACTYPE");
        }

        //OnAddValidators操作执行前，加载操作校验器
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            purOrderValidator validator = new purOrderValidator();
            //是否需要校验,true需要
            validator.AlwaysValidate = true;
            //校验单据头
            validator.EntityKey = "FBillHead";
            //加载校验器
            e.Validators.Add(validator);
        }
        //自定义校验器.派生:AbstractValidator
        private class purOrderValidator : AbstractValidator
        {
            //重写方法
            //数组ExtendedDataEntity,传递全部的信息
            public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
            {   //for循环,读取数据
                foreach (ExtendedDataEntity obj in dataEntities)
                {
                    String erpContractType = Convert.ToString(obj["F_SRT_CONTRACTYPE"]);
                    if (!"KJ".EqualsIgnoreCase(erpContractType))
                    {

                        DynamicObjectCollection poorderFinances = obj["POOrderEntry"] as DynamicObjectCollection;
                        //判断复选框是否勾选
                        if (poorderFinances.Count <= 0)
                        {   //报错
                            validateContext.AddError(obj.DataEntity,
                                new ValidationErrorInfo
                                ("",//出错的字段Key，可以空
                                obj.DataEntity["Id"].ToString(),// 数据包内码，必填，后续操作会据此内码避开此数据包
                                obj.DataEntityIndex, // 出错的数据包在全部数据包中的顺序
                                obj.RowIndex,// 出错的数据行在全部数据行中的顺序，如果校验基于单据头，此为0
                                "001",//错误编码，可以任意设定一个字符，主要用于追查错误来源
                                "编码：" + obj.BillNo + "客户可行额度不足,请调整再提交！",// 错误的详细提示信息
                                "必须有一行明细记录" + obj.BillNo + "，客户可行额度不足,请调整再提交！",// 错误的简明提示信息
                                Kingdee.BOS.Core.Validation.ErrorLevel.Error// 错误级别：警告、错误...
                                ));
                        }
                    }
                }
            }
        }
    }
}
