<?php
/**
 * @copyright ICZ Corporation (http://www.icz.co.jp/)
 * @license See the LICENCE file
 * @author <matcha@icz.co.jp>
 * @version $Id$
 */
require('simple_coverpage.php');
class BILLPDF extends SIMPLE_COVERPAGEPDF
{

	//ヘッダ
	function Header()
	{
	}

	function main($_param, $_county,$_accounttype,$_direction,$_items,$_pages,$_billtype){
   		$this->Line(18, 148.5 , 190, 148.5);
		$this->SetLineWidth(0.2);

        for($p = 0 ; $p <= 1; $p++){
        $BaseY = 148 * $p + 8;

        //社判
		if($_param['Company']['SEAL'] && $_param['Bill']['CMP_SEAL_FLG']){
            if ($p == 0) {
    			$this->Image('/volume1/web/matcha-invoice/app/tmp/img/stamp3.png',172, 42 + $BaseY - (16 - $p) , 25,0);
            }
		}
			$this->Image('/volume1/web/matcha-invoice/app/tmp/img/CSM_LOGO.jpg',135, 10 + (($BaseY - 7) * $p), 55,0);



		$this->SetMargins(0, 0, 0);
		$this->SetAutoPageBreak(true, 1.0);

		switch($_param['Company']['COLOR']) {
			//黒
			case 0:
                $line_color = array('C' => 100, 'M' => 100, 'Y' => 100, 'K' => 100);
                $column_color = array('C' => 0, 'M' => 0, 'Y' => 0, 'K' => 30);
                $row_color = array('C' => 0, 'M' => 0, 'Y' => 0, 'K' => 10);
                $header_color = array('C' => 0, 'M' => 0, 'Y' => 0, 'K' => 20);

                // $line_color = array('R' => 1, 'G' => 1, 'B' => 1);
                // $column_color = array('R' => 220, 'G' => 220, 'B' => 220);
                // $row_color = array('R' => 238, 'G' => 238, 'B' => 238);
				// $column_color = array('R' => 204, 'G' => 204, 'B' => 204);
				// $row_color = array('R' => 238, 'G' => 238, 'B' => 238);
//				$line_color = array('R' => 136, 'G' => 136, 'B' => 136);
//				$column_color = array('R' => 204, 'G' => 204, 'B' => 204);
//				$row_color = array('R' => 238, 'G' => 238, 'B' => 238);
				break;
			//青
			case 1:
				$line_color = array('R' => 0, 'G' => 99, 'B' => 244);
				$column_color = array('R' => 135, 'G' => 179, 'B' => 230);
				$row_color = array('R' => 212, 'G' => 237, 'B' => 255);
				break;

			//赤
			case 2:
				$line_color = array('R' => 255, 'G' => 89, 'B' => 158);
				$column_color = array('R' => 255, 'G' => 181, 'B' => 184);
				$row_color = array('R' => 255, 'G' => 240, 'B' => 255);
				break;

			//緑
			case 3:
				$line_color = array('R' => 0, 'G' => 88, 'B' =>52);
				$column_color = array('R' => 160, 'G' => 217, 'B' => 168);
				$row_color = array('R' => 223, 'G' => 242, 'B' => 226);
				break;
		}
      
        $this->SetDrawColor($line_color['C'], $line_color['M'], $line_color['Y'], $line_color['K']);
		$this->SetFont(MINCHO,'B',16);
		$this->SetXY(18, 17 - $BaseY);

//		if ($_billtype == 0 )
		if ($p == 0 )
        {
            $str = "御 請 求 書";
        }
        else
        {
            $str = "御 請 求 書 （控）";
        }
		
		$str = $this->conv($str);
//		$this->SetFillColor($column_color['R']);
		$this->Cell( 172, 10, $str, 'B', 1, 'L',0);
		$this->SetTextColor(0);

		//No.
		$this->SetXY(152, 29 - $BaseY);
		$this->SetFont(MINCHO,'',9);
		$str = 'No.';
		$str = $this->conv($str);
		$this->Write(5, $str);

		//見積書番号
		$this->SetXY(168, 29 - $BaseY);
		$this->SetFont(MINCHO,'',9);
		$str = $_param['Bill']['NO'];
		$str = $this->conv($str);
		$this->Cell( 23, 5, $str, 0, 1, 'R');

		//日付
		$this->SetXY(170,33 - $BaseY);
		$this->SetFont(MINCHO,'',9);
		$str = substr($_param['Bill']['ISSUE_DATE'],0,4)."年".substr($_param['Bill']['ISSUE_DATE'],5,2)."月".substr($_param['Bill']['ISSUE_DATE'],8,2)."日";
		$str = $this->conv($str);
		$this->Cell( 21, 5, $str, 0, 1, 'R');

        $TY = 5;
		//部署・顧客担当者名

			$this->SetXY(25, 40 - $BaseY - $TY);
			$this->SetFont(MINCHO,'',$this->customer_font(mb_strlen($_param['Customer']['NAME'])));
			$str = " ".$_param['Customer']['NAME'];
			switch($_param['Bill']['HONOR_CODE'] ) {
				case 0:
					$str .= '　御中';
					break;

				case 1:
					$str .= '　様';
					break;

				case 2:
					$str .= '　'.$_param['Bill']['HONOR_TITLE'];
					break;
			}
			$str = $this->conv($str);
			$this->Cell( 60, 7, $str, 'B');



		$temp_y = 55.5;
        $this->SetDrawColor($line_color['C'], $line_color['M'], $line_color['Y'], $line_color['K']);
		//自社名
		$this->writeCompanyInfo($_param, $_county, $_x, 24 + $BaseY + ($p * 1) );

        $BaseY = $BaseY + 10;

		//合計金額
//		$this->SetXY(18, 80 - $BaseY);
		$this->SetXY(25, 65 - $BaseY - $TY);
		$this->SetLineWidth(0.4);
		$this->SetFont(MINCHO,'B',13);
		$str = "合計金額";
		$str = $this->conv($str);
		$this->Cell( 100, 7, $str, 0, 1, 'L');

//		$this->SetXY(18, 80 - $BaseY);
		$this->SetXY(25, 64 - $BaseY - $TY);
		$this->SetFont(MINCHO,'B',18);
		$str = '\\'.number_format($_param['Bill']['TOTAL']).'-　';
		$str = $this->conv($str);
		$this->Cell( 90, 8, $str, 'B', 1, 'R');

        $this->Line(25, 50 + (($BaseY - 17) * $p) , 115, 50 + (($BaseY - 17) * $p));
		$this->SetLineWidth(0.2);

        //下記の通り御請求申し上げます。
		$this->SetXY(28, 75 - $BaseY - $TY);
		$this->SetFont(MINCHO,'',10);
		$this->SetDrawColor($line_color['C'], $line_color['M'], $line_color['Y'], $line_color['K']);
		$str = "下記の通り".$_param['Bill']['SUBJECT']."御請求申し上げます。";
		$str = $this->conv($str);
		$this->Write(5, $str);

		//振込期限
		if(!empty($_param['Bill']['DUE_DATE'])){
			$this->SetXY(18, 84 - $BaseY - $TY);
			$this->SetFont(MINCHO,'',9);
			$str = "振込期限:".$_param['Bill']['DUE_DATE'];
			$str = $this->conv($str);   
			$this->Cell( 80, 4.5, $str, 'B', 1, 'L');
		}
        $BaseY = $BaseY + 5;
		//単位・円
//		$this->SetXY(175, 87 - $BaseY);       
		$this->SetXY(175, 85 - $BaseY);
		$this->SetFont(MINCHO,'',10);
		$str = "単位：円";
		$str = $this->conv($str);
		$this->Write(5, $str);

		//表の幅
// 		$w_no		=  8.5;
// 		$w_item		= 85.5;
// 		$w_quantity	=  23 ;
// 		$w_unit		=  19 ;
// 		$w_total	=  36 ;

		$w_no		=  8;
		$w_code		= 15;
		$w_item		= 73;
		$w_quantity	= 23;
		$w_unit		= 23;
		$w_total	= 30;

		// 項目数(改ページなどの非項目を含まない数)
		$num_item_count = 0;
		$max_item_count = 0;
		for($i = 0; isset($_param[$i]); $i++) {
			$fbreak = isset($_param[$i]['Billitem']['LINE_ATTRIBUTE'])
			&& intval($_param[$i]['Billitem']['LINE_ATTRIBUTE']) == 8; // 改ページ
			if (!$fbreak) {
				$max_item_count++;
			}
		}
		//複数あるか税率があるかを確認
		$tax_kinds = array('TEN_RATE_TOTAL','REDUCED_RATE_TOTAL','EIGHT_RATE_TOTAL','FIVE_RATE_TOTAL');
		$tax_kind_count = 0;
		foreach($tax_kinds as $key){
			if($_param['Bill'][$key]){
				$tax_kind_count++;
			}
		}
        $BaseY = $BaseY +2;
		//表の表示
		for($j = 0 ; $j < $_pages; $j++){
			$amount=0;
			$discount_m=0;
			$item_count=0;
			$rows=0;
			//1ページのみの場合
			if($_pages == 1 && $j == 0){
//            for($t = 0 ; $t < 2; $t++){
                $BaseY = $BaseY + 200 * $t;
				$page_num = 8;
                $row_height = 5;
				$this->SetXY(18, 92 - $BaseY);
				$this->SetFont(MINCHO,'B',8);
                $this->SetDrawColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);

				// $this->SetDrawColor($line_color['C'], $line_color['M'], $line_color['Y'], $line_color['K']);
				$this->SetFillColor($header_color['C'], $header_color['M'], $header_color['Y'], $header_color['K']);
				$this->Cell($w_no, $row_height, $this->conv('No.'), 'T', 0, 'C',1);
				$this->Cell($w_code, $row_height, $this->conv('商品コード'), 'T', 0, 'C',1);
				$this->Cell($w_item, $row_height, $this->conv('品目名'), 'T', 0, 'C',1);
				$this->Cell($w_quantity, $row_height, $this->conv('数量'), 'T', 0, 'C',1);
				$this->Cell($w_unit, $row_height, $this->conv('単価'), 'T', 0, 'C',1);
				$this->Cell($w_total, $row_height, $this->conv('合計'), 'T', 0, 'C',1);
				$this->SetFont(MINCHO,'',$row_height);
				$i = 0;

//				$y = 222;
				$y = 150;

				if($_param['Bill']['DISCOUNT_TYPE'] != 2){
					if($_param['Bill']['REDUCED_RATE_TOTAL']){
						$this->SetXY(17, $y - $BaseY);
						$this->SetFont(MINCHO,'',8);
						$str = "「※」は軽減税率対象であることを示します。";
						$str = $this->conv($str);
						$this->Write(5, $str);
					}
					$this->SetXY(123, $y - $BaseY);
					$this->SetFont(MINCHO,'B',8);
					$this->SetFillColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
					$this->Cell(28, 6, $this->conv('　割引'), '1', 0, 'L',1);
					$this->SetFont(MINCHO,'',8);
					$this->SetFillColor(0,0,0,0);

					if($_param['Bill']['DISCOUNT_TYPE'] == 0 ){
						$str = $_param['Bill']['DISCOUNT'] ? number_format($_param['Bill']['DISCOUNT'] ) .'%': '0';
					}elseif($_param['Bill']['DISCOUNT_TYPE'] == 1 ){
						$str = $_param['Bill']['DISCOUNT'] ? '▲'.number_format($_param['Bill']['DISCOUNT'] ) : '0';
					}else{
						$str ='';
					}

					$str = $this->conv($str);
					$this->Cell(39, 6, $str, 1, 0, 'R',1);
					$y = $y + 6;
				}elseif($_param['Bill']['REDUCED_RATE_TOTAL']){
					$this->SetXY(17, $y - $BaseY);
					$this->SetFont(MINCHO,'',8);
					$str = "「※」は軽減税率対象であることを示します。";
					$str = $this->conv($str);
					$this->Write(5, $str);
					$y = $y + 6;
				}

				if($tax_kind_count > 1){
					$tax_kind_x = 18;
					$tax_kind_y = $y;
					$this->SetXY($tax_kind_x, $y - $BaseY);
					$this->Cell( 105, 18, '', 1, 1, 'C');
					$this->SetXY($tax_kind_x, $y - $BaseY);
					$this->SetFont(MINCHO,'',7);
					$str = '・内訳';
					$str = $this->conv($str);
					$this->SetXY($tax_kind_x, $y - $BaseY);
					$this->Write(5, $str);
					if($_param['Bill']['TEN_RATE_TOTAL']){
						$tax_kind_y = $tax_kind_y + 4;
						$this->SetXY($tax_kind_x, $tax_kind_yv - $BaseY);
						$str = '10%対象      '.number_format($_param['Bill']['TEN_RATE_TOTAL']);
						$str .= ' (消費税'.number_format($_param['Bill']['TEN_RATE_TAX']).')';
						$str = $this->conv($str);
						$this->Write(5, $str);
					}
					if($_param['Bill']['REDUCED_RATE_TOTAL']){
						$tax_kind_y = $tax_kind_y + 3;
						$this->SetXY($tax_kind_x, $tax_kind_y - $BaseY);
						$str = '8%(軽減)対象 '.number_format($_param['Bill']['REDUCED_RATE_TOTAL']);
						$str .= ' (消費税'.number_format($_param['Bill']['REDUCED_RATE_TAX']).')';
						$str = $this->conv($str);
						$this->Write(5, $str);
					}
					if($_param['Bill']['EIGHT_RATE_TOTAL']){
						$tax_kind_y = $tax_kind_y + 3;
						$this->SetXY($tax_kind_x, $tax_kind_y - $BaseY);
						$str = '8%対象       '.number_format($_param['Bill']['EIGHT_RATE_TOTAL']);
						$str .= ' (消費税'.number_format($_param['Bill']['EIGHT_RATE_TAX']).')';
						$str = $this->conv($str);
						$this->Write(5, $str);
					}
					if($_param['Bill']['FIVE_RATE_TOTAL']){
						$tax_kind_y = $tax_kind_y + 3;
						$this->SetXY($tax_kind_x, $tax_kind_y - $BaseY);
						$str = '5%対象       '.number_format($_param['Bill']['FIVE_RATE_TOTAL']);
						$str .= ' (消費税'.number_format($_param['Bill']['FIVE_RATE_TAX']).')';
						$str = $this->conv($str);
						$this->Write(5, $str);
					}
				}
                $y = $y - 12;
				//備考欄
//				$this->SetXY(18, $y - $BaseY);
				$this->SetXY(18, $y - $BaseY);
				$this->SetFont(MINCHO,'B',9);
				$str = "備考欄";
				$str = $this->conv($str);
				$this->Write(5, $str);
//				$y = $y + 5;
				$this->SetFont(MINCHO,'',9);
				$this->SetDrawColor($line_color['C'], $line_color['M'], $line_color['Y'], $line_color['K']);
				$this->SetXY(18, $y - $BaseY);
				$str = $_param['Bill']['NOTE'];
				$str = $this->conv($str);
//				$this->Cell( 172, 25, '', 1, 1, 'C');
				$this->Cell( 120, 20, '', 1, 1, 'C');
				$this->SetXY(18, $y+1 - $BaseY);
				$this->MBMultiCell(172, 4, $str, 0, 'L');
				$this->SetFont(MINCHO,'B',8);
                
                //合計欄
//				$this->SetXY(123, $y - $BaseY);
				$this->SetXY(140, $y - $BaseY);
				$this->SetFont(MINCHO,'B',8);
				$this->SetFillColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
//				$this->Cell(28, 6, $this->conv('　小計'), '1', 0, 'L',1);
				$this->Cell(20, 6, $this->conv('　小計'), '1', 0, 'L',1);
				$this->SetFillColor(0,0,0,0);
				$this->SetFont(MINCHO,'',8);
				$str = $_param['Bill']['SUBTOTAL'] ? number_format($_param['Bill']['SUBTOTAL']) : '0';
				$str = $this->conv($str);
//				$this->Cell(39, 6, $str, 1, 0, 'R',1);
				$this->Cell(30, 6, $str, 1, 0, 'R',1);
				$y = $y + 6;

//				$this->SetXY(123, $y - $BaseY);
				$this->SetXY(140, $y - $BaseY);
				$this->SetFont(MINCHO,'B',8);
				$this->SetFillColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
//				$this->Cell(28, 6, $this->conv('　合計'), '1', 0, 'L',1);
				$this->Cell(20, 6, $this->conv('　合計'), '1', 0, 'L',1);
				$this->SetFillColor(0,0,0,0);
				$this->SetFont(MINCHO,'',8);
				$str = $_param['Bill']['TOTAL'] ? number_format($_param['Bill']['TOTAL']) : '0';
				$str = $this->conv($str);
//				$this->Cell(39, 6, $str, 1, 0, 'R',1);
				$this->Cell(30, 6, $str, 1, 0, 'R',1);
				$y = $y + 16;



//				$y = $y + 26;

							//振込先
				$this->SetXY(18, $y - $BaseY);
				$this->SetFont(MINCHO,'B',9);
				$str = "振込先：";
				$acount = '';
				if(isset($_param['Company']['ACCOUNT_TYPE'])) $acount = $_accounttype[$_param['Company']['ACCOUNT_TYPE']];
				$str = "振込先：".$_param['Company']['ACCOUNT_HOLDER']."　".$_param['Company']['BANK_NAME'].
				"　".$_param['Company']['BANK_BRANCH']."　".$acount."　".$_param['Company']['ACCOUNT_NUMBER'];
				$str = $this->conv($str);
				$this->Cell( 172, 5, $str, 'B', 1, 'L');

//            }
			}

            $this->SetFont(MINCHO,'',8);

			$max_item_per_page = 8;
			$fwrite_param = true;
			for($rows=0 ; $rows < $page_num ; ){
				if($j == 0){
					if($rows % 2 == 0) {
						$this->SetXY(18, (96 + $rows * 5 + 0.25) - $BaseY);
						$this->SetFillColor(0,0,0,0);
						$height = 5;
						$border = "";
					}else {
						$this->SetXY(18, (96 + $rows * 5+0.15) - $BaseY);
						$this->SetFillColor($row_color['C'], $row_color['M'], $row_color['Y'], $row_color['K']);
						$this->SetDrawColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
						$height = 5;
						$border = "TB";
					}
				}

				else{
					if($rows % 2 == 0) {
						$this->SetXY(18, (25 + $rows * 6 + 0.5) - $BaseY);
						$this->SetFillColor(0,0,0,0);
						$height = 4.5;
						$border = "";
					}else {
						$this->SetXY(18, (25 + $rows * 6) - $BaseY);
						$this->SetFillColor($row_color['C'], $row_color['M'], $row_color['Y'], $row_color['K']);
						$this->SetDrawColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
						$height = 5.5;
						$border = "TB";
					}
				}

				//改ページの場合はこれ以上現在のページに書き出さない.
				$fbreak = isset($_param[$i]['Billitem']['LINE_ATTRIBUTE'])
				&& intval($_param[$i]['Billitem']['LINE_ATTRIBUTE']) == 8;
				if ($fbreak) {
					$fwrite_param = false;
					$i++;
				}

				//No.
				$str = '';
				if ($fwrite_param) {
					$str = isset($_param[$i]['Billitem']['ITEM_NO'])?$_param[$i]['Billitem']['ITEM_NO']:'';
					$str = $this->conv($str);
				}
				$this->Cell($w_no, $height, $str, $border, 0, 'C',1);

				//商品コード
				$str = '';
				if ($fwrite_param) {
					$str = isset($_param[$i]['Billitem']['ITEM_CODE'])?$_param[$i]['Billitem']['ITEM_CODE']:'';
					$str = $this->conv($str);
				}
				$this->Cell($w_code, $height, $str, $border, 0, 'C',1);

				//品目名
				$str = '';
				if ($fwrite_param) {
					$str = isset($_param[$i]['Billitem']['ITEM'])?$_param[$i]['Billitem']['ITEM']:'';
					//軽減税率対象の商品の場合
					if($_param[$i]['Billitem']['TAX_CLASS'] == 91 || $_param[$i]['Billitem']['TAX_CLASS'] == 92){
						$str .= '(※)';
					}
					$str = $this->conv($str);
				}
				$this->Cell($w_item, $height, $str, $border, 0, 'L',1);

				//数量
				$str = '';
				if ($fwrite_param) {
					$str = isset($_param[$i]['Billitem']['QUANTITY']) && isset($_param[$i]['Billitem']['UNIT'])
					?$this->ht2br($_param[$i]['Billitem']['QUANTITY'],null,'QUANTITY').$_param[$i]['Billitem']['UNIT']:'';
					$str = $this->conv($str);
				}
				$this->Cell($w_quantity, $height, $str, $border, 0, 'C',1);

				//単価
				$str = '';
				if ($fwrite_param) {
					$str .= isset($_param[$i]['Billitem']['UNIT_PRICE']) ? $this->ht2br($_param[$i]['Billitem']['UNIT_PRICE'],null,'UNIT_PRICE'):'';
					$str = $this->conv($str);
				}
				$this->Cell($w_unit, $height, $str, $border, 0, 'R',1);


				//合計
				$str = '';
				if ($fwrite_param) {
					if(isset($_param[$i]['Billitem']['TAX_CLASS']) && $_param[$i]['Billitem']['TAX_CLASS']%10 == 1) $str = '(内)';
					if(isset($_param[$i]['Billitem']['TAX_CLASS']) && $_param[$i]['Billitem']['TAX_CLASS']%10 == 3) $str = '';
					$str .= isset($_param[$i]['Billitem']['AMOUNT']) ? number_format($_param[$i]['Billitem']['AMOUNT']):'';
					$str = $this->conv($str);
				}
				$this->Cell($w_total, $height, $str, $border, 0, 'R',1);

				// アイテムのカウンタ
				if($fwrite_param) {
					$i++;
					$num_item_count++;
				}

				// 行のカウンタ
				$rows++;

				// ページの最後の10ページに含まれる改ページ数をカウントする(この数だけページが多くなっている).
				$break_count = 0;
				if(($j == 0 && $rows == 8) || ($j != 0 && $rows == 30)) {
					for ($k = 0; $k < ($rows+10); $k++) {
						$fbreak = isset($_param[$i+$k]) && isset($_param[$i+$k]['Billitem']['LINE_ATTRIBUTE'])
						&& intval($_param[$i+$k]['Billitem']['LINE_ATTRIBUTE']) == 8;
						if ($fbreak) {
							$break_count++;
						}
					}
				}

				// 最後のページの１つ前が、項目数が残り10以下の場合は次のページに送る.
				$remain_count = $max_item_count - $num_item_count;
				if(($_pages > 1) && ($j == $_pages - 2 - $break_count)
				&& (0 < $remain_count && $remain_count <= 10)
				&& (($j == 0 && $rows == 8) || ($j != 0 && $rows == 8))) {
					if ($j == 0) {
						$max_item_per_page = 8;
					} else {
						$max_item_per_page = 8;
					}
				}

				// 今のページに書き出した項目数が上限値に達したら、このページでの書き込みを停止する.
				if ($rows >= $max_item_per_page) {
					$fwrite_param = false;
				}

				if($j == 0){
					if($rows % 2 == 0) {
						$this->SetXY(18, (99 + $rows * 6 + 0.5) - $BaseY);
						$this->SetFillColor(0,0,0,0);
						$height = 5.5;
						$border = "";
					}else {
						$this->SetXY(18, (99 + $rows * 6) - $BaseY);
						$this->SetFillColor($row_color['C'], $row_color['M'], $row_color['Y'], $row_color['K']);
						$this->SetDrawColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
						$height = 6;
						$border = "TB";
					}
				}

				else{
					if($rows % 2 == 0) {
						$this->SetXY(18, (25 + $rows * 6 + 0.5) - $BaseY);
						$this->SetFillColor(0,0,0,0);
						$height = 5.5;
						$border = "";
					}else {
						$this->SetXY(18, (25 + $rows * 6) - $BaseY);
						$this->SetFillColor($row_color['C'], $row_color['M'], $row_color['Y'], $row_color['K']);
						$this->SetDrawColor($column_color['C'], $column_color['M'], $column_color['Y'], $column_color['K']);
						$height = 6;
						$border = "TB";
					}
				}
				if(isset($_param[$i]['Billitem']['DISCOUNT']) && $_param[$i]['Billitem']['DISCOUNT']){
					//No.
					$str = '';
					$str = $this->conv($str);
					$this->Cell($w_no, $height, $str, $border, 0, 'C',1);

					//品目名
					$str = '割引';
					$str = $this->conv($str);
					$this->Cell($w_item, $height, $str, $border, 0, 'L',1);

					//数量
					$str = '';
					$str = $this->conv($str);
					$this->Cell($w_quantity, $height, $str, $border, 0, 'C',1);

					//単価
					$str = $_param[$i]['Billitem']['DISCOUNT_TYPE']==1?'':$_param[$i]['Billitem']['DISCOUNT'].'%';
					$str = $this->conv($str);
					$this->Cell($w_unit, $height, $str, $border, 0, 'R',1);

					//合計
					$str = '▲'.number_format($_param[$i]['Billitem']['DISCOUNT_TYPE']==0?$_param[$i]['Billitem']['AMOUNT']*($_param[$i]['Billitem']['DISCOUNT']*0.01):$_param[$i]['Billitem']['DISCOUNT']);
					$str = $this->conv($str);
					$this->Cell($w_total, $height, $str, $border, 0, 'R',1);
					$amount-=($_param[$i]['Billitem']['DISCOUNT_TYPE']==0?$_param[$i]['Billitem']['AMOUNT']*($_param[$i]['Billitem']['DISCOUNT']*0.01):$_param[$i]['Billitem']['DISCOUNT']);
					$item_count++;
					$rows++;
				}
				$item_count++;
			}


			if($j != $_pages - 1) {
                $this->AddPage();
            }
		}
        }
    }
    
			//フッター
	function Footer()
	{
    }
}
?>