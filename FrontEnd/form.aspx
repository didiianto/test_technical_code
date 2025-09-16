<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="form.aspx.cs" Inherits="FrontEnd.formn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
        <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>
<body>
    <form id="bookingForm" >
        <div>
     <label>Room:
      <select id="meetingRoomId">
        <option value="1">Room A (5 orang)</option>
        <option value="2">Room B (10 orang)</option>
      </select>
    </label><br>
    <label>Start:
      <input type="datetime-local" id="start" required>
    </label><br>
    <label>Duration (minutes):
      <input type="number" id="durationMinutes" min="15" step="15" max="120" required>
    </label><br>
    <label>People:
      <input type="number" id="peeople" min="1" max="10" required>
    </label><br>
    <button type="submit">Book</button>
        </div>
    </form>

    <h2>Schedules</h2>
  <table border="1" id="scheduleTable"></table>

    <div id="suggestions" style="margin-top:20px; color:red;"></div>
<script>
    const API_URL = "http://localhost:7142/api/meetingrooms"; 

    async function loadSchedule() {
        let res = await fetch(API_URL);
        let rooms = await res.json();
        let table = document.getElementById("scheduleTable");
        table.innerHTML = "<tr><th>Room</th><th>Bookings</th></tr>";
        rooms.forEach(r => {
            let bookings = (r.bookings || []).map(b =>
                `${new Date(b.start).toLocaleTimeString()} - ${new Date(b.end).toLocaleTimeString()} (${b.peeople} orang)`
            ).join("<br>");
            table.innerHTML += `<tr><td>${r.name} (max ${r.capacity})</td><td>${bookings}</td></tr>`;
        });
    }

    document.getElementById("bookingForm").onsubmit = async e => {
        e.preventDefault();
        document.getElementById("suggestions").innerHTML = "";

        let req = {
            meetingRoomId: parseInt(document.getElementById("meetingRoomId").value),
            start: document.getElementById("start").value,
            durationMinutes: parseInt(document.getElementById("durationMinutes").value),
            peeople: parseInt(document.getElementById("peeople").value)
        };

        let res = await fetch(API_URL, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(req)
        });

        if (res.ok) {
            alert("Booking sukses!");
            loadSchedule();
        } else {
            let errText = await res.text(); 
            let err;
            try {
                err = JSON.parse(errText); 
            } catch {

            }

            if (err && err.suggestions) {
                let html = `<p>${err.message}</p><ul>`;
                err.suggestions.forEach(s => {
                    html += `<li>${s.roomName}: ${new Date(s.suggestedStart).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - ${new Date(s.suggestedEnd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</li>`;
                });
                html += "</ul>";
                document.getElementById("suggestions").innerHTML = html;
            } else {
                alert("Booking gagal: " + (err?.message || errText));
            }
        }
    };

    loadSchedule();
</script>
</body>
</html>