using System.Globalization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"config.json")).Build();
var req = new RequestBuilder(config["Token"]);
var client = new LoggingGraphQLHttpClient(config["Endpoint"]);


var orderId = Guid.NewGuid();
var deliveryId = Guid.NewGuid();
var paymentId = Guid.NewGuid();

Log.WriteLine();
Log.WriteLine("Creating order...", ConsoleColor.Yellow);
Log.WriteLine();

var createCommands = new List<object>{
  new {
    Command = "AddDelivery",
    DeliveryId = deliveryId,
    DeliveryMethodId = Guid.Parse(config["DeliveryMethodId"]),
    InventoryId = Guid.Parse(config["InventoryId"]),
    DeliveryAddress = new {
      AddressId = Guid.NewGuid(),
      FirstName = config["Customer:DeliveryAddress:FirstName"],
      LastName = config["Customer:DeliveryAddress:LastName"],
      OrganizationName = config["Customer:DeliveryAddress:OrganizationName"],
      Address1 = config["Customer:DeliveryAddress:Address1"],
      Address2 = config["Customer:DeliveryAddress:Address2"],
      City = config["Customer:DeliveryAddress:City"],
      PostalCode = config["Customer:DeliveryAddress:PostalCode"],
      CountryCode = config["Customer:DeliveryAddress:CountryCode"],
      State = config["Customer:DeliveryAddress:State"],
      Email = config["Customer:DeliveryAddress:Email"],
      PhoneNumber = config["Customer:DeliveryAddress:Phone"]
    }
  },
  new {
    Command = "AddRow",
    DeliveryId = deliveryId,
    newRowId = Guid.NewGuid(),
    ArticleNumber = config["Row:ProductNumber"],
    Quantity = decimal.Parse(config["Row:Quantity"], CultureInfo.InvariantCulture),
  }
};

var res = await client.SendMutationAsync<dynamic>(
  req.Build(@"mutation {
  hantera {
    orders {
      createOrder(
        channelId:""" + config["ChannelId"] + @"""
        countryCode:""" + config["Customer:DeliveryAddress:CountryCode"] + @"""
        currencyCode:""" + config["CurrencyCode"] + @"""
        customerId:""" + config["Customer:Id"] + @"""
        orderId:""" + orderId + @"""
        commandsJson: """ + JsonConvert.SerializeObject(createCommands).Replace("\"", "\\\"") + @"""
      )
      {
        orderNumber
        inactiveInvoice {
          invoiceId
        }
      }
    } 
  }
}"));

if (res == null)
{
  return;
}

var invoiceId = Guid.Parse((string)res.hantera.orders.createOrder.inactiveInvoice.invoiceId);

Log.WriteLine("Order created", ConsoleColor.Yellow);
Log.WriteLine("  OrderNumber: " + (string)res.hantera.orders.createOrder.inactiveInvoice.orderNumber, ConsoleColor.Yellow);
Log.WriteLine("  InvoiceId: " + invoiceId, ConsoleColor.Yellow);

Log.WriteLine();
Log.WriteLine("Creating payment...", ConsoleColor.Yellow);
Log.WriteLine();

res = await client.SendMutationAsync<dynamic>(
  req.Build(@"mutation {
  hantera {
    orders {
      createPayment(
        orderId:""" + orderId + @"""
        paymentId:""" + paymentId + @"""
        invoiceId:""" + invoiceId + @"""
        provider:""" + config["Payment:Provider"] + @"""
        method:""" + config["Payment:Method"] + @"""
        transactionReference:""" + config["Payment:TransactionReference"] + @"""
        amount:" + config["Payment:ReservedAmount"] + @"
        reservedAmount:" + config["Payment:ReservedAmount"] + @"
      )
      {
        paymentNumber
      }
    } 
  }
}"));

if (res == null)
{
  return;
}

Log.WriteLine("Payment created", ConsoleColor.Yellow);
Log.WriteLine("  PaymentNumber: " + (string)res.hantera.orders.createPayment.paymentNumber, ConsoleColor.Yellow);

Log.WriteLine();
Log.WriteLine("Updating order...", ConsoleColor.Yellow);
Log.WriteLine();

var commands = new List<object> {
  new {
    Command = "MarkOrderAsConfirmed",
  }
};

if (bool.Parse(config["Actions:Ship"]))
{
  commands.Add(new
  {
    Command = "MarkDeliveryAsPicked",
    DeliveryId = deliveryId,
  });

  commands.Add(new
  {
    Command = "MarkDeliveryAsShipped",
    DeliveryId = deliveryId,
  });
}

if (bool.Parse(config["Actions:Invoice"]))
{
  commands.Add(new
  {
    Command = "ActivateInvoice",
    DeliveryIds = new[] { deliveryId },
  });
}

res = await commitOrderCommands(commands);

if (res == null)
{
  return;
}

Log.WriteLine();
Log.WriteLine("Order updated", ConsoleColor.Yellow);
Log.WriteLine();

//var result = (String)res.Data.hantera.orders.createOrder.inactiveInvoice.invoiceId;



async Task<dynamic?> commitOrderCommands(List<dynamic> commands)
{

  return await client.SendMutationAsync<dynamic>(
    req.Build(@"mutation {
    hantera {
      orders {
        commitCommands(
          orderId:""" + orderId + @"""
          commandsJson: """ + JsonConvert.SerializeObject(commands).Replace("\"", "\\\"") + @"""
        )
        {
          orderNumber
          state
          deliveries {
            deliveryNumber
            state
            rows {
              article {
                articleNumber
                name
              }
              quantity
            }
          }
          invoices {
            invoiceNumber
            invoiceTotal
          }
          payments {
            reservedAmount
            paidAmount
          }
        }
      } 
    }
  }"));
}