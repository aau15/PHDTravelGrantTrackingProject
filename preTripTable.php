<!DOCTYPE html>
<html>

<?php
@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());
$conn = new mysqli('mysql5006.smarterasp.net', 'a117c6_phd', 'phd12345', 'db_a117c6_phd');

//$StundentID = $_SESSION["StudentID"];

$sql = "SELECT DoFReg, CostOfTrip, ConferenceName FROM studenttrip, trip WHERE trip.tripID = studenttrip.tripID AND StudentID = 'cl415'";
$result = $conn->query($sql);
?>

<body>

<h1>3. Previous Funded Trips</h1>

<form method="post" action="preTripTable.php">
    <table border=1>
        <tr>
            <td width="5%"></td>
            <td width="20%">Date of Claim ( YYYY-MM-DD )</td>
            <td width="25%">Amount Received (Nearest in pound)</td>
            <td width="50%">Conference</td>

            <?php
            //$arr = array();
            //$row = mysql_fetch_array($result);
            $total_cost = 0;
            $oneyear_cost = 0;
            $last_date;

            if ($result->num_rows > 0) {
                $row_num = 0;
                while($row = $result->fetch_assoc()) {
                    $row_num ++;
                    //echo "<br> DoFReg: ". $row["DoFReg"]. " - CostOfTrip: ". $row["CostOfTrip"]." - ConferenceName: ". $row["ConferenceName"] ;
                    echo "<tr><td>".$row_num ."</td><td>".$row["DoFReg"]."</td><td>".$row["CostOfTrip"]."</td><td>".$row["ConferenceName"]."</td>";
                    $total_cost += $row["CostOfTrip"];
                    //$last_date = $row["DoFReg"];
                }
                $date = date_sub(date_create(date("Y-m-d",time())), date_interval_create_from_date_string("12 months"));
                echo date_format($date,"Y-m-d");
            } else {
                echo "0";
            }
            $conn->close();
            ?>
    </table>
    <br/><br/>
    <table border=1>
        <tr>
            <td width="25%">Total funds received (T)</td>
            <td width="25%"><?php echo $total_cost; ?></td>
            <td width="50%">Total Allowance Remaining: 3000-T = <?php echo 3000-$total_cost; ?></td>
        <tr>
            <td width="25%">Funds received in the last 12 months (Y)</td>
            <td width="25%"><?php echo $total_cost; ?></td>
            <td width="50%">12 Month Allowance Remaining: 2000-T = <?php echo 2000-$total_cost; ?></td>
    </table>
    <br>
    <br>
    <button type="submit" name="submit" style="float:right">submit</button>
	<?php 
		if (isset($_POST['submit'])){
		echo "<script>window.location='estimatedCostTable.html';</script>";
	}?>
</form>

</body>
</html>

