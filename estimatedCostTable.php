<?php

session_cache_limiter('private, must-revalidate');
@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());

if (isset($_POST['submit'])){
	/* $conn = new mysqli('mysql5006.smarterasp.net', 'a117c6_phd', 'phd12345', 'db_a117c6_phd');
	if ($conn -> connect_error) {
		die('Could not connect: ' . $conn -> connect_error);
	}else { */
		
		$rfCost = $_POST['rfECost'];
		$tcCost = $_POST['tcECost'];
		$aCost = $_POST['aECost'];
		$mCost = $_POST['mECost'];
		$oCost = $_POST['oECost'];
		


		if(!is_numeric($rfCost)||!is_numeric($tcCost)||!is_numeric($aCost)||!is_numeric($mCost)||!is_numeric($oCost)) {
			echo "<script>alert('Please ensure all cost inputs are numbers.'); history.go(-1);</script>";
		}
		else{
			$total = $rfCost+$tcCost+$aCost+$mCost+$oCost;
				
		 	$rfCal = $_POST['rfCal'];
			$tcCal = $_POST['tcCal'];
			$aCal = $_POST['aCal'];
			$mCal = $_POST['mCal'];
			$oCal = $_POST['oCal']; 
			
			$_SESSION["rfCal"]=$rfCal;  $_SESSION["rfCost"]=$rfCost;
			$_SESSION["tcCal"]=$tcCal;  $_SESSION["tcCost"]=$tcCost;
			$_SESSION["aCal"]=$aCal;    $_SESSION["aCost"]=$aCost;
			$_SESSION["mCal"]=$mCal;    $_SESSION["mCost"]=$mCost;
			$_SESSION["oCal"]=$oCal;    $_SESSION["oCost"]=$oCost;
			$_SESSION["totalECost"] = $total;
			
			/* $query = "insert into ecost
			(RegFeeCal, RegFee, TransFeeCal, TransFee, AccFeeCal, AccFee, MealFeeCal, MealFee, OtherFeeCal, OtherFee, TripId, TotalECost)
			values('{$rfCal}','{$rfCost}','{$tcCal}','{$tcCost}','{$aCal}',
			'{$aCost}','{$mCal}','{$mCost}','{$oCal}','{$oCost}','{$_SESSION["tripID"]}','{$total}')";
				
			$result=$conn->query($query); */
			echo "<script>window.location='stuAppConfirm.php';</script>";
				
		}
		//$conn->close();

	//}
}