<?php
@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());

if (isset($_POST['submit'])){
	$conn = new mysqli('mysql5006.smarterasp.net', 'a117c6_phd', 'phd12345', 'db_a117c6_phd');
	if ($conn -> connect_error) {
		die('Could not connect: ' . $conn -> connect_error);
	}else {
		$query_stu = "insert into studenttrip
		(StudentID, TripID, FeePayer, FeeLPaid, DoFReg, MaxClaim)
		values('{$_SESSION["StudentID"]}','{$_SESSION["tripID"]}','{$_SESSION["feePayer"]}','{$_SESSION["date_feeLPaid"]}',
		'{$_SESSION["date_fReg"]}',1111111)";
		
		$result=$conn->query($query_stu);
		
		$query_trip = "insert into trip
		(TripID, ConferenceName, ConferenceURL, City, Country,
		PaperTitle, Author, Purpose, Type, Status, CostOfTrip)
		values('{$_SESSION["tripID"]}',
		'{$_SESSION["cName"]}','{$_SESSION["cURL"]}','{$_SESSION["city"]}','{$_SESSION["country"]}','{$_SESSION["ptitle"]}',
		'{$_SESSION["authors"]}','{$_SESSION["purpose"]}','{$_SESSION["type"]}','{$_SESSION["status"]}',1)";
		
		$result=$conn->query($query_trip);
		
		$query_ec = "insert into ecost
		(RegFeeCal, RegFee, TransFeeCal, TransFee, AccFeeCal, AccFee, MealFeeCal, MealFee, OtherFeeCal, OtherFee, TripId, TotalECost)
		values('{$_SESSION["rfCal"]}','{$_SESSION["rfCost"]}','{$_SESSION["tcCal"]}','{$_SESSION["tcCost"]}','{$_SESSION["aCal"]}',
		'{$_SESSION["aCost"]}','{$_SESSION["mCal"]}','{$_SESSION["mCost"]}','{$_SESSION["oCal"]}','{$_SESSION["oCost"]}',
		'{$_SESSION["tripID"]}','{$_SESSION["totalECost"]}')";
		
		$result=$conn->query($query_ec); 
	//echo "<script>window.location='stuAppConfirm.php';</script>";
		echo "ok";
	}
	$conn->close();

	
}