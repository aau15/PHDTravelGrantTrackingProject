<?php

// keep previous user input when crashes happen or users click "back"
session_cache_limiter('private, must-revalidate'); 
@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());

if (isset($_POST['submit'])){
	
	/* $conn = new mysqli('mysql5006.smarterasp.net', 'a117c6_phd', 'phd12345', 'db_a117c6_phd');
	if ($conn -> connect_error) {
		die('Could not connect: ' . $conn -> connect_error);
	}else { */
		// student name, email  直接从数据库读？	
		$stuName = $_POST['stuName']; $_SESSION["stuName"]=$stuName;
		$email = $_POST['email']; $_SESSION["email"]=$email;
		$fRegD = $_POST['fRegD']; //$_SESSION["fRegD"]=$fRegD;	
		$fRegM = $_POST['fRegM']; //$_SESSION["fRegM"]=$fRegM;
		$fRegY = $_POST['fRegY']; //$_SESSION["fRegY"]=$fRegY; 
		$monthCom = $_POST['monthCom']; $_SESSION["monthCom"]=$monthCom;
		$feePayer = $_POST['feePayer']; $_SESSION["feePayer"]=$feePayer;
		 $feeLPaidD = $_POST['feeLPaidD']; //$_SESSION["feeLPaidD"]=$feeLPaidD;
		$feeLPaidM = $_POST['feeLPaidM']; //$_SESSION["feeLPaidM"]=$feeLPaidM;
		$feeLPaidY = $_POST['feeLPaidY']; //$_SESSION["feeLPaidY"]=$feeLPaidY; 
		
		if(!is_numeric($monthCom)||!is_numeric($fRegM)||!is_numeric($fRegD)||
				!is_numeric($fRegY)||!is_numeric($feeLPaidM)||!is_numeric($feeLPaidD)||
				!is_numeric($feeLPaidY)) {
			echo "<script>alert('Please ensure all month and year inputs are numbers.'); history.go(-1);</script>";
		}
		else{
			// 有问题
			date_default_timezone_set('UTC');
			$date_fReg = date("Y-m-d",strtotime($fRegY."-".$fRegM."-".$fRegD));
			$date_feeLPaid = date("Y-m-d",strtotime($feeLPaidY."-".$feeLPaidM."-".$feeLPaidD));
			$_SESSION["date_fReg"]=$date_fReg;
			$_SESSION["date_feeLPaid"]=$date_feeLPaid;
			
		//	$tripID = $_
		
			//这里 studentid, tripid, maxclaim要讨论， month completed要添加
			/* $query = "insert into studenttrip
			(StudentID, TripID, FeePayer, FeeLPaid, DoFReg, MaxClaim)
			values('{$_SESSION["StudentID"]}','{$_SESSION["tripID"]}','{$feePayer}','{$date_feeLPaid}','{$date_fReg}',1111111)";
				
			$result=$conn->query($query); */
			echo "<script>window.location='tripDetailsTable.html';</script>";
			
		}
		//$conn->close();

	//}
}