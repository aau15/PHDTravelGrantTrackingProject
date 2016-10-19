<?php @session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());?>


<html>
<h1>Confirmation: Travel Funding Application </h1>
<body>


<h2>Details of Student</h2>
<?php echo "Student Name:  ".$_SESSION["stuName"];?>
<br></br>
<?php echo "Email:  ".$_SESSION["email"];?>
<br></br>
<?php echo "Date of 1st Registration:  ".$_SESSION["date_fReg"];?>
<br></br>
<?php echo "Months Completed  :".$_SESSION["ptitle"];?>
<br></br>
<?php echo "Fees Paid by:  ".$_SESSION["feePayer"];?>
<br></br>
<?php echo "Fees Last Paid  :".$_SESSION["date_feeLPaid"];?>

<br></br>

<h2>Details of Trip</h2>
<?php if ($_SESSION["type"]==0){?>
<?php echo "Conference Name:  ".$_SESSION["cName"];?>
<br></br>
<?php echo "Conference URL:  ".$_SESSION["cURL"];?>
<br></br>
<?php echo "City/Country:  ".$_SESSION["city"]." / ".$_SESSION["country"];?>
<br></br>
<?php echo "Paper Title  :".$_SESSION["ptitle"];?>
<br></br>
<?php echo "Authors  :".$_SESSION["authors"];?>
<br></br>
<?php } else{?>
<?php echo "Purpose:  ".$_SESSION["purpose"];}?>

<br></br>


<h2>Estimated Cost</h2>
<?php echo "Registration Fee:  ".$_SESSION["rfCost"];?>
<br></br>
<?php echo "Transport Costs:  ".$_SESSION["tcCost"];?>
<br></br>
<?php echo "Accomodition:  ".$_SESSION["aCost"];?>
<br></br>
<?php echo "Meals:  ".$_SESSION["mCost"];?>
<br></br>
<?php echo "Other Items:  ".$_SESSION["oCost"];?>
<br></br>
<?php echo "Total (E):  ".$_SESSION["totalECost"];?>
<br></br>
<br></br>
<form method="post" action="dbOperation.php">
<button type="submit" name="submit" style="float:right">Confirm</button>
</form>

</body>
</html>
