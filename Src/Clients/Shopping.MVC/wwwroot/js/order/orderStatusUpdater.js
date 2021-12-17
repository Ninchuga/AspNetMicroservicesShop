
let connection = null;

setupConnection = () => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5004/orderstatushub") // order api url
        .build();

    connection.on("OrderStatusUpdated", (orderId, orderStatus) => {

        const orderStatusElement = document.getElementById(`${orderId}_orderStatus`);
        orderStatusElement.innerHTML = orderStatus;
    });

    connection.start()
        .then(() => console.log("Connection started"))
        .catch(error => consoler.log("Error while starting connection: " + error.ToString()));

    connection.on("finished", function () {
        connection.stop();
    });
};

setupConnection();