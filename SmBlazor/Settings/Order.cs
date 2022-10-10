using Microsoft.AspNetCore.Components;
using SmQueryOptionsNs;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Emit;

namespace SmBlazor
{
    public class Order
    {
        public List<OrderField> OrderFields { get; set; } = new List<OrderField>();

        /// <summary>
        /// átrendezi a várt sorrendet a korábbi beállításól, a kattintott mezőtől, és a kattintáskori shift állapottól függően
        /// </summary>
        public void SetOrder(string fieldName, bool shiftKey)
        {
            if (!shiftKey && OrderFields.Count > 1)
            {
                //Console.WriteLine("ha shift nélkül kattintok, és előtte egynéltöbb rendezési szempont volt, akkor a korábbi rendezési szempontokat törlöm");
                OrderFields.Clear();
            }

            //ha eddig nem volt semmi order
            if (OrderFields.Count == 0)
            {
                OrderFields.Add(new OrderField() { FieldName = fieldName, Descending = false });
                //Console.WriteLine("ha eddig nem volt semmi order");
                return;
            }
            //normál esetben az utolsó rendezési szempontot piszkálom
            var lastOrder = OrderFields.Last();

            //Ha az utolsó szempont más mezőre vonatkozott, de nem shifttel kattintottam, akkor lecserélem arra
            if (lastOrder.FieldName != fieldName && !shiftKey)
            {
                lastOrder.FieldName = fieldName;
                lastOrder.Descending = false;
                //Console.WriteLine("Ha az utolsó szempont más mezőre vonatkozott, de nem shifttel kattintottam, akkor lecserélem arra");
                return;
            }
            //Ha az utolsó szempont más mezőre vonatkozott, és shifttel kattintottam, de a mező szerepl az order mezők között akkor annak fordítom a sorrendjét
            var oldFieldOrder = OrderFields.SingleOrDefault(x => x.FieldName == fieldName);
            if (lastOrder.FieldName != fieldName && shiftKey && oldFieldOrder != null)
            {
                ChangeFieldsOrder(oldFieldOrder);
                //Console.WriteLine("Ha az utolsó szempont más mezőre vonatkozott, és shifttel kattintottam, de a mező szerepl az order mezők között akkor annak fordítom a sorrendjét");

                return;
            }
            //Ha az utolsó szempont más mezőre vonatkozott, és shifttel kattintottam, és a mező nem szerepl az order mezők között akkor felveszem azt a lista végére
            if (lastOrder.FieldName != fieldName && shiftKey && oldFieldOrder == null)
            {
                OrderFields.Add(new OrderField() { FieldName = fieldName, Descending = false });
                //Console.WriteLine("Ha az utolsó szempont más mezőre vonatkozott, és shifttel kattintottam, akkor felveszem azt a lista végére");

                return;
            }
            ChangeFieldsOrder(lastOrder);

            //sorrendfordító
            void ChangeFieldsOrder(OrderField orderField)
            {
                //Ha az utolsó szempont növekvő volt ugyanezen a mezőn, akkor csökkenőre állítom
                if (!orderField.Descending)
                {
                    orderField.Descending = true;
                    Console.WriteLine("Ha az utolsó szempont növekvő volt ugyanezen a mezőn, akkor csökkenőre állítom");
                    return;
                }
                //Ha az utolsó szempont csökkenő volt ugyanezen a mezőn, akkor törlöm a szempontot
                if (orderField.Descending)
                {
                    OrderFields.Remove(orderField);
                    Console.WriteLine("Ha az utolsó szempont csökkenő volt ugyanezen a mezőn, akkor törlöm a szempontot");
                }
            }
        }

        public (int? Nth, bool Descending)? ColumnOrderInfo(string fieldName)
        {
            var nth = 1;
            if (OrderFields.Count == 1)
            {
                var orderField = OrderFields.First();
                if (fieldName == orderField.FieldName)
                    return (null, orderField.Descending);
            }
            foreach (var orderField in OrderFields)
            {
                if (fieldName == orderField.FieldName)
                    return (nth, orderField.Descending);
                nth += 1;
            }
            return null;
        }
    }
}
