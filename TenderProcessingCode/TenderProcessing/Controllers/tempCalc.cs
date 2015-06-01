using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TenderProcessing.Views.Calc
{
    public class tempCalc
    {
         ////////row++;
         //////                   var rowValid = true;
         //////                   var controlCell = workSheet.Cell(row, 1);



         //////                   bool f =controlCell.IsMerged();


         //////                   //определение типа строки
         //////                   //var rowType = RowType.CalculateHeader;
         //////                   var controlValue = controlCell.Value;
         //////                   if (controlValue != null)
         //////                   {
         //////                       var controlValueString = controlValue.ToString();
         //////                       if (string.IsNullOrEmpty(controlValueString))
         //////                       {
         //////                           //строка расчета
         //////                           //rowType = RowType.CalculateRow;
         //////                           errorStringBuilder.Append("Не найден идентификатор позиции в строке: " + row + "<br/>");
         //////                           break;
         //////                       }
         //////                       else
         //////                       {
         //////                           ////заголовок позиции
         //////                           //if (controlValueString == "Id")
         //////                           //{
         //////                           //    emptyRowCount = 0;
         //////                           //    rowType = RowType.PositionHeader;
         //////                           //    if (model != null)
         //////                           //    {
         //////                           //        positions.Add(model);
         //////                           //    }
         //////                           //    model = new SpecificationPosition() { Calculations = new List<CalculateSpecificationPosition>(), Author = user.Id};
         //////                           //}
         //////                           //else if (controlValueString == "callHd")
         //////                           //{
         //////                           //    //заголовок расчета
         //////                           //    emptyRowCount = 0;
         //////                           //    rowType = RowType.CalculateHeader;
         //////                           //}
         //////                           //else
         //////                           //{
         //////                               //получение Id позиции
         //////                               emptyRowCount = 0;
         //////                               int id;
         //////                               var converting = int.TryParse(controlValueString, out id);
         //////                               if (converting)
         //////                               {
         //////                                   //if (model == null)
         //////                                   //{
         //////                                       model = new SpecificationPosition() { Calculations = new List<CalculateSpecificationPosition>(), Author = user.Id };
         //////                                       //errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
         //////                                       //break;
         //////                                   //}
         //////                                   model.Id = id;
         //////                                   //rowType = RowType.PositionRecord;
         //////                                   readyToParse = true;
         //////                               }
         //////                               else
         //////                               {
         //////                                   errorStringBuilder.Append("Ошибка разбора Id позиции в строке: " + row + "<br/>");
         //////                                   //rowType = RowType.PositionRecord;
         //////                                   readyToParse = false;
         //////                               }
         //////                           //}
         //////                       }
         //////                   }
         //////                   //else
         //////                   //{
         //////                   //    rowType = RowType.CalculateRow;
         //////                   //}
         //////                   //if (rowType == RowType.CalculateHeader)
         //////                   //{
         //////                   //    //готовность к разбору инфы по расчету
         //////                   //    readyToParse = true;
         //////                   //}
         //////                   //if (rowType == RowType.PositionHeader || rowType == RowType.PositionRecord)
         //////                   //{
         //////                   //    readyToParse = false;
         //////                   //}
         //////                   //if (rowType != RowType.CalculateRow)
         //////                   //{
         //////                   //    continue;
         //////                   //}
         //////                   //if (!readyToParse)
         //////                   //{
         //////                   //    errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
         //////                   //    break;
         //////                   //}
         //////                   //else
         //////                   if (readyToParse)
         //////                   {
         //////                       //разбор инфы по расчету к позиции

         //////                       //Если строка запроса пустая то Конец
         //////                       if (String.IsNullOrEmpty(workSheet.Cell(row, 3).Value.ToString().Trim())) { break;}
                                
         //////                       //Если строка расчета не пустая, то парсим ее
         //////                       bool flag4Parse = false;
         //////                       for (int i = 4; i <= 15; i ++)
         //////                       {
         //////                           if (!String.IsNullOrEmpty(workSheet.Cell(row, i).Value.ToString().Trim()))
         //////                           {
         //////                               flag4Parse = true;
         //////                               break;
         //////                           }
         //////                       }

         //////                       if (flag4Parse)
         //////                       {
                                    
         //////                           //var nameValueString = nameValue.ToString().Trim();
         //////                           //if (string.IsNullOrEmpty(nameValueString))
         //////                           //{
         //////                           //    emptyRowCount++;
         //////                           //    if (emptyRowCount > 1)
         //////                           //    {
         //////                           //        if (model != null)
         //////                           //        {
         //////                           //            positions.Add(model);
         //////                           //        }
         //////                           //        break;
         //////                           //    }
         //////                           //    continue;
         //////                           //}
         //////                           if (model == null)
         //////                           {
         //////                               errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
         //////                               break;
         //////                           }
         //////                           calculate = new CalculateSpecificationPosition()
         //////                           {
         //////                               IdSpecificationPosition = model.Id,
         //////                               IdTenderClaim = claimId,
         //////                               Author = user.Id
         //////                           };
         //////                           //получение значений расчета из ячеек
         //////                           var catalogValue = workSheet.Cell(row, 4).Value;
         //////                           var nameValue = workSheet.Cell(row, 5).Value;
         //////                           var replaceValue = workSheet.Cell(row, 6).Value;
         //////                           var priceUsd = workSheet.Cell(row, 7).Value;
         //////                           var priceEur = workSheet.Cell(row, 8).Value;
         //////                           var priceEurRicoh = workSheet.Cell(row, 9).Value;
         //////                           var priceRubl = workSheet.Cell(row, 10).Value;

         //////                           //var priceCurrencyValue = workSheet.Cell(row, 4).Value;
         //////                           //var sumCurrencyValue = workSheet.Cell(row, 5).Value;
         //////                           //var currencyValue = workSheet.Cell(row, 6).Value;
         //////                           ////var priceRubValue = workSheet.Cell(row, 7).Value;
         //////                           ////var sumRubValue = workSheet.Cell(row, 9).Value;
         //////                           var providerValue = workSheet.Cell(row, 11).Value;
         //////                           var deliveryTimeValue = workSheet.Cell(row, 12).Value;
         //////                           var protectFactValue = workSheet.Cell(row, 13).Value;
         //////                           var protectConditionValue = workSheet.Cell(row, 14).Value;
         //////                           var commentValue = workSheet.Cell(row, 15).Value;

         //////                           //Проверка
         //////                           if (deliveryTimeValue != null && string.IsNullOrEmpty(deliveryTimeValue.ToString().Trim()))
         //////                           {
         //////                               rowValid = false;
         //////                               errorStringBuilder.Append("Строка: " + row +
         //////                                                     ", не задано обязательное значение Срок поставки<br/>");
         //////                               break;
         //////                           }
         //////                           if ((priceUsd != null && string.IsNullOrEmpty(priceUsd.ToString().Trim()))
         //////                               && (priceEur != null && string.IsNullOrEmpty(priceEur.ToString().Trim()))
         //////                               &&
         //////                               (priceEurRicoh != null && string.IsNullOrEmpty(priceEurRicoh.ToString().Trim()))
         //////                               && (priceRubl != null && string.IsNullOrEmpty(priceRubl.ToString().Trim())))
         //////                           {
         //////                               rowValid = false;
         //////                               errorStringBuilder.Append("Строка: " + row +
         //////                                                     ", не указано ни одной цены<br/>");
         //////                               break;
         //////                           }

         //////                           //Заполняем
         //////                           calculate.CatalogNumber = catalogValue.ToString();
         //////                           calculate.Name = nameValue.ToString();
         //////                           calculate.Replace = replaceValue.ToString();

         //////                           double prUsd;
         //////                           if (!String.IsNullOrEmpty(priceUsd.ToString().Trim()) && double.TryParse(priceUsd.ToString().Trim(), out prUsd))
         //////                           {
         //////                               calculate.PriceUsd = prUsd;
         //////                           }

         //////                           double prEur;
         //////                           if (!String.IsNullOrEmpty(priceEur.ToString().Trim()) && double.TryParse(priceEur.ToString().Trim(), out prEur))
         //////                           {
         //////                               calculate.PriceEur = prEur;
         //////                           }

         //////                           double prEurRicoh;
         //////                           if (!String.IsNullOrEmpty(priceEurRicoh.ToString().Trim()) && double.TryParse(priceEurRicoh.ToString().Trim(), out prEurRicoh))
         //////                           {
         //////                               calculate.PriceEurRicoh = prEurRicoh;
         //////                           }

         //////                           double prRubl;
         //////                           if (!String.IsNullOrEmpty(priceRubl.ToString().Trim()) && double.TryParse(priceRubl.ToString().Trim(), out prRubl))
         //////                           {
         //////                               calculate.PriceEurRicoh = prRubl;
         //////                           }

         //////                           calculate.Provider = providerValue.ToString();

         //////                           var delivertTimeValueString = deliveryTimeValue.ToString().Trim();
         //////                           var possibleDelTimValues = deliveryTimes.Select(x => x.Value);
         //////                           if (!possibleDelTimValues.Contains(delivertTimeValueString))
         //////                           {
         //////                               rowValid = false;
         //////                               errorStringBuilder.Append("Строка: " + row +
         //////                                                     ", Значение '" + delivertTimeValueString + "' не является допустимым для Срок поставки<br/>");
         //////                           }
         //////                           else
         //////                           {
         //////                               var delTime = deliveryTimes.First(x => x.Value == delivertTimeValueString);
         //////                               calculate.DeliveryTime = delTime;
         //////                           }

         //////                           var protectFactValueString = protectFactValue.ToString().Trim();
         //////                           var possibleValues = protectFacts.Select(x => x.Value);
         //////                           if (!possibleValues.Contains(protectFactValueString))
         //////                           {
         //////                               rowValid = false;
         //////                               errorStringBuilder.Append("Строка: " + row +
         //////                                                     ", Значение '" + protectFactValueString + "' не является допустимым для Факт получ.защиты<br/>");
         //////                           }
         //////                           else
         //////                           {
         //////                               var fact = protectFacts.First(x => x.Value == protectFactValueString);
         //////                               calculate.ProtectFact = fact;
         //////                           }


         //////                           #region hide

         //////                           ////проверка: Каталожный номер
         //////                           //if (catalogValue == null || string.IsNullOrEmpty(catalogValue.ToString().Trim()))
         //////                           //{
         //////                           //    rowValid = false;
         //////                           //    errorStringBuilder.Append("Строка: " + row +
         //////                           //                          ", не задано обязательное значение Каталожный номер<br/>");
         //////                           //}
         //////                           //else
         //////                           //{
         //////                           //    calculate.CatalogNumber = catalogValue.ToString().Trim();
         //////                           //}
         //////                           ////проверка: Факт получ.защиты и условия защиты
         //////                           //if (protectFactValue == null || string.IsNullOrEmpty(protectFactValue.ToString().Trim()))
         //////                           //{
         //////                           //    rowValid = false;
         //////                           //    errorStringBuilder.Append("Строка: " + row +
         //////                           //                          ", не задано обязательное значение Факт получ.защиты<br/>");
         //////                           //}
         //////                           //else
         //////                           //{
         //////                           //    var protectFactValueString = protectFactValue.ToString().Trim();
         //////                           //    var possibleValues = protectFacts.Select(x => x.Value);
         //////                           //    if (!possibleValues.Contains(protectFactValueString))
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                              ", Значение '" + protectFactValueString + "' не является допустимым для Факт получ.защиты<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        var fact = protectFacts.First(x => x.Value == protectFactValueString);
         //////                           //        calculate.ProtectFact = fact;
         //////                           //        if (protectFactValueString != "Не предоставляется")
         //////                           //        {
         //////                           //            if (protectConditionValue == null ||
         //////                           //                string.IsNullOrEmpty(protectConditionValue.ToString().Trim()))
         //////                           //            {
         //////                           //                rowValid = false;
         //////                           //                errorStringBuilder.Append("Строка: " + row +
         //////                           //                                          ", не задано обязательное значение Условия защиты<br/>");
         //////                           //            }
         //////                           //            else
         //////                           //            {
         //////                           //                calculate.ProtectCondition = protectConditionValue.ToString().Trim();
         //////                           //            }
         //////                           //        }
         //////                           //        else
         //////                           //        {
         //////                           //            if (protectConditionValue != null &&
         //////                           //                !string.IsNullOrEmpty(protectConditionValue.ToString().Trim()))
         //////                           //            {
         //////                           //                calculate.ProtectCondition = protectConditionValue.ToString().Trim();
         //////                           //            }
         //////                           //        }
         //////                           //    }
         //////                           //}
         //////                           //проверка: Сумма вход руб
         //////                           //if (sumRubValue != null || !string.IsNullOrEmpty(sumRubValue.ToString().Trim()))
         //////                           //{
         //////                           //    double doubleValue;
         //////                           //    var isValidDouble = double.TryParse(sumRubValue.ToString().Trim(), out doubleValue);
         //////                           //    if (!isValidDouble)
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                                  ", значение '" + sumRubValue.ToString().Trim() + "' в поле Сумма вход руб не является числом<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        calculate.SumRub = doubleValue;
         //////                           //    }
         //////                           //}
         //////                           //проверка: Цена за ед. USD
         //////                           //if (priceCurrencyValue != null && !string.IsNullOrEmpty(priceCurrencyValue.ToString().Trim()))
         //////                           //{
         //////                           //    double doubleValue;
         //////                           //    var isValidDouble = double.TryParse(priceCurrencyValue.ToString().Trim(), out doubleValue);
         //////                           //    if (!isValidDouble)
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                                  ", значение '" + priceCurrencyValue.ToString().Trim() + "' в поле Цена за ед. не является числом<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        calculate.PriceCurrency = doubleValue;
         //////                           //    }
         //////                           //}
         //////                           ////проверка: Сумма вход USD
         //////                           //if (sumCurrencyValue != null && !string.IsNullOrEmpty(sumCurrencyValue.ToString().Trim()))
         //////                           //{
         //////                           //    double doubleValue;
         //////                           //    var isValidDouble = double.TryParse(sumCurrencyValue.ToString().Trim(), out doubleValue);
         //////                           //    if (!isValidDouble)
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                                  ", значение '" + sumCurrencyValue.ToString().Trim() + "' в поле Сумма вход не является числом<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        calculate.SumCurrency = doubleValue;
         //////                           //    }
         //////                           //}
         //////                           ////проверка валюта
         //////                           //if (currencyValue != null && !string.IsNullOrEmpty(currencyValue.ToString().Trim()))
         //////                           //{
         //////                           //    var currency =
         //////                           //        currencies.FirstOrDefault(x => x.Value == currencyValue.ToString().Trim());
         //////                           //    if (currency == null)
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                              ", Значение '" + currencyValue.ToString().Trim() + "' не является допустимым для Валюта<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        calculate.Currency = currency.Id;
         //////                           //    }
         //////                           //}
         //////                           //else
         //////                           //{
         //////                           //    if (!model.Price.Equals(0) || !model.Sum.Equals(0))
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                                  ", не задано обязательное значение Валюта<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        calculate.Currency = 1;
         //////                           //    }
         //////                           //}
         //////                           ////проверка: Цена за ед. руб
         //////                           //if (priceRubValue != null && !string.IsNullOrEmpty(priceRubValue.ToString().Trim()))
         //////                           //{
         //////                           //    double doubleValue;
         //////                           //    var isValidDouble = double.TryParse(priceRubValue.ToString().Trim(), out doubleValue);
         //////                           //    if (!isValidDouble)
         //////                           //    {
         //////                           //        rowValid = false;
         //////                           //        errorStringBuilder.Append("Строка: " + row +
         //////                           //                                  ", значение '" + priceRubValue.ToString().Trim() + "' в поле Цена за ед. руб не является числом<br/>");
         //////                           //    }
         //////                           //    else
         //////                           //    {
         //////                           //        calculate.PriceRub = doubleValue;
         //////                           //    }
         //////                           //}

         //////                           #endregion

         //////                           //if (replaceValue != null && !string.IsNullOrEmpty(replaceValue.ToString().Trim()))
         //////                           //{
         //////                           //    calculate.Replace = replaceValue.ToString().Trim();
         //////                           //}
         //////                           //if (providerValue != null && !string.IsNullOrEmpty(providerValue.ToString().Trim()))
         //////                           //{
         //////                           //    calculate.Provider = providerValue.ToString().Trim();
         //////                           //}
         //////                           //if (commentValue != null && !string.IsNullOrEmpty(commentValue.ToString().Trim()))
         //////                           //{
         //////                           //    calculate.Comment = commentValue.ToString().Trim();
         //////                           //}
         //////                       }
         //////                       else
         //////                       {
         //////                           emptyRowCount++;
         //////                           if (emptyRowCount > 1)
         //////                           {
         //////                               if (model != null)
         //////                               {
         //////                                   positions.Add(model);
         //////                               }
         //////                               break;
         //////                           }
         //////                           continue;
         //////                       }
         //////                   }
         //////                   if (rowValid)
         //////                   {
         //////                       if (calculate != null && model != null)
         //////                       {
         //////                           model.Calculations.Add(calculate);
         //////                       }
         //////                   }

         //////                   row++;
    }
}