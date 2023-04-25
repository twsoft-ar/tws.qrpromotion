// *************************************************************************
//                       MICROS RES3700 QR PROMOTION                       
//						    Current Version: 1.0.8
// *************************************************************************
// *************************************************************************
//
//
// *************************************************************************
// *   EDITOR SETTINGS                                                     *
// *   ====== ========                                                     *
// *                                                                       *
// *   Set tabulation to 4 chars long to visualize a correct indentation   *
// *                                                                       *
// *************************************************************************
//
// =========================================================================
// History
// =========================================================================
//
// 04/04/2023 - v1.0.8 * Luis Vaccaro * 
//
// 31/03/2023 - v1.0.7 * Luis Vaccaro * 
//
// 15/03/2023 - v1.0.6 * Luis Vaccaro * 
//
// 26/11/2022 - v1.0.1 * Luis Vaccaro * 
//
// 10/03/2021 - v1.0.0 * Luis Vaccaro * 
//
//
// =========================================================================
// TO DO
//
// =========================================================================
//
// =========================================================================

// ------------------------------------------------------------- //
///////////////////////////////////////////////////////////////////
//			The Following code is not to be modified!			 //
///////////////////////////////////////////////////////////////////
// ------------------------------------------------------------- //

// SIM OPTIONS
RetainGlobalVar
SetSignOnLeft 

// --------------------------------------------------------- //
///////////////////////// Constants ///////////////////////////
// --------------------------------------------------------- //

Var IFC_VERSION									:A16 = "QRP 1.0.8"
                        						
//CONSTANTS        
Var TRUE										:N1 = 1
Var FALSE										:N1 = 0
                                       			
Var PCWS_TYPE									:N1 = 1	// Type number for PCWS
Var PPC55A_TYPE 					 			:N1 = 2 // Type number for Workstation mTablet
Var WS4_TYPE									:N1 = 3	// Type number for Workstation 4
Var WS5_TYPE									:N1 = 3	// Type number for Workstation 5
                            					
 
// Check detail types [@dtl_type]

Var DT_CHECK_INFO     							:A1 = "I"
Var DT_MENU_ITEM		     					:A1 = "M"
Var DT_DISCOUNT       							:A1 = "D"
Var DT_SERVICE_CHARGE 							:A1 = "S"
Var DT_TENDER				 					:A1 = "T"
Var DT_REFERENCE      							:A1 = "R"
Var DT_CA_DETAIL      							:A1 = "C"


//Detail status flag definitions
// 1ST DIGIT
Var DTL_PREV_RND								:N1 = 1
Var DTL_RETURN									:N1 = 4
// 2ND DIGIT                			
Var DTL_VOID									:N1 = 5
// 3RD DIGIT                			
Var DTL_PRV_RND_VOID							:N2 = 10
Var DTL_ERROR_CORRECT							:N2 = 12
// 4TH DIGIT                			
Var DTL_ITEM_SHARED								:N2 = 14
Var DTL_VOID_EDIT								:N2 = 15
// 8TH DIGIT					    			
Var DTL_SKU_ENTRY								:N2 = 30
                            			
// Detail TypeDef bits      			
Var DTL_ITEM_PRICE_PRESET						:N2 = 1
Var DTL_ITEM_IS_WEIGHED							:N2 = 28 
// ** Protocol ENUMS **
 
//Key Types
Var KEY_TYPE_MENU_ITEM_OBJNUM 					:N1 = 3
Var KEY_TYPE_DISCOUNT_OBJNUM 					:N1 = 5
Var KEY_TYPE_TENDER_OBJNUM 						:N1 = 9
  
////////////////////////// IFC Global vars /////////////////////////////
Var MAX_ITEM_ARRAY_SIZE 						:N9 = 64

Var gbliSelectedPrinterIndex 					:N9
Var SELECTED_PRINTER[4] 						:N9
	SELECTED_PRINTER[1] = @RCPT
	SELECTED_PRINTER[2] = @CHK
	SELECTED_PRINTER[3] = @JNL
	SELECTED_PRINTER[4] = @VALD

Var gbliWSType				 					:N1		// To store the current Workstation type  
Var gbliRESMajVer								:N2		// To store the current RES major version 
Var gbliRESMinVer								:N2		// To store the current RES minor version
     
// filenames                    	
Var PATH_TO_GEOITD_DRIVER						:A100
Var ERROR_LOG_FILE_NAME							:A100
Var CONFIGURATION_FILE_NAME						:A100
Var CONFIGURATION_FILE_NAME_PWS					:A100
                                				                             				
// Driver handles (DLLs)        				
Var gblhQRPromotionDrv 							:N12

// IFC Specific
Var gblsStoreId 								:A64 	// Store Identifier
Var gblsServerIP 								:A16 	// Micros ReqServer IP (micros res server ip)
Var gbliServerPort 								:N9 	// Micros ReqServer PORT
Var gbliQRPromoItemObjNum 						:N9		// Obj Num of the item inserted to indicate a promo is in progress
Var gbliQRPromoEndItemObjNum 					:N9		// Obj Num of the last item inserted to indicate a promo is end
//Var gbliItemObjNum[MAX_ITEM_ARRAY_SIZE] 		:N9
//Var gbliItemType[MAX_ITEM_ARRAY_SIZE] 			:N9
//Var gbliItemArraySize 							:N9

Var gblsQRCodeArray[MAX_ITEM_ARRAY_SIZE] 		:A32
Var gbliQRCodeQtyArray[MAX_ITEM_ARRAY_SIZE] 	:N9
Var gbliQRCodeArrayCount 						:N9


Var MENU_ITEM 									:N1 = 1
Var DISCOUNT 									:N1 = 2

//Req Server Error codes
Var INVALID_MGS 								:N9 = -1
Var EXECUTION_OK								:N9 = 0   // 0
Var EXECUTION_ERROR								:N9 = 156 //-100 
Var CONNECTION_ERROR							:N9 = 155 //-101	
Var PARAM_ERROR 								:N9 = 154 //-102
Var TIMEOUT_ERROR								:N9 = 153 //-103
Var CHECKSUM_ERROR		 						:N9 = 152 //-104

//QR Promotion Error codes
Var QR_VALID 									:N9 = 0
Var QR_NOT_FOUND 								:N9 = 1
Var QR_NOT_PAUSED 								:N9 = 2
Var QR_OUT_STOCK 								:N9 = 3
Var QR_EXPIRED 									:N9 = 4
Var QR_BURNED 									:N9 = 5
Var QR_INTERNAL_ERROR 							:N9 = 6
Var QR_WS_ENDPOINT_ERROR 						:N9 = -2
Var QR_FAIL 									:N9 = -1


////////////////////////////////////////////////////////////////
//							EVENTS							  //
////////////////////////////////////////////////////////////////

// ****************************************************************************************
// * NOTE: if this isl is executed as stand alone, uncomment this events and define every *
// *       single on of it; otherwise, assure that caller isl has this events defined and *
// *       calls every function as they are called here.                                  *
// ****************************************************************************************

Event Init
	
	// get client platform
	Call setWorkstationType()

	// set file paths for this client
	Call setFilePaths()

	// Load Custom Settings from .cfg file
	Call LoadCustomSettings()

	Call RemoveOrphanVouchers(TRUE)

	InfoMessage "QR PROMO INTERFACE",IFC_VERSION
	
EndEvent

Event Final_Tender	
	Call RedeemQR()
	Call ClearGobalVars()
EndEvent

Event Trans_Cncl		
	Call RemoveOrphanVouchers(TRUE)
	Call ClearGobalVars()
EndEvent

Event Tndr
EndEvent

Event Dtl_Changed
	Call RemoveOrphanVouchers(FALSE)
EndEvent

////////////////////////////////////////////////////////////////
//                         INQ EVENTS                         //
////////////////////////////////////////////////////////////////

Event Inq : 1 //Process QR Coupon

	Call ProcessQR()	

EndEvent

Event Inq : 15 //TEST
	
	Format PATH_TO_GEOITD_DRIVER 		As "..\bin\TWS.RES.QRClientW32.dll"		
	Call LoadQRPromotionDrv()
	ErrorMessage "gblhQRPromotionDrv=", gblhQRPromotionDrv

	Var status    :N9 
	Var size      :N9 = 100
	Var type[100] :N9
	Var plu[100]  :N9
	Var qty[100]  :N9
	Var res       :N9
	Var errorMsg  :A1024

	DLLCall_CDecl gblhQRPromotionDrv, Test(ref plu[])
	DLLCall_CDecl gblhQRPromotionDrv, Test2()

	DLLCall_CDecl gblhQRPromotionDrv, Validate("127.0.0.1", 7511, "AXW-456-PROM4", Ref size, Ref status, Ref type[], Ref plu[], Ref qty[], 1024, Ref errorMsg, Ref res)
	ErrorMessage "size=", size, " status=", status, " res=", res 
	Var index : N9 
	
	For index = 1 to size
		InfoMessage "T:", type[index], " P:", plu[index], " Q:", qty[index]
	EndFor

EndEvent


///////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                       //
//=========================================== IFC FUNCTIONS =============================================//
//                                                                                                       //
///////////////////////////////////////////////////////////////////////////////////////////////////////////

//******************************************************************
// Procedure: ProcessQR()
//******************************************************************
Sub ProcessQR()
	
	Var status 							:N9
	Var qrPresent 						:N9
	Var retVal 							:N9
	Var qrStr 							:A32
	Var qrStrAux 						:A32
	Var itemArraySize 					:N9 = MAX_ITEM_ARRAY_SIZE
	Var itemType[MAX_ITEM_ARRAY_SIZE] 	:N9
	Var itemObjNum[MAX_ITEM_ARRAY_SIZE] :N9
	Var itemQty[MAX_ITEM_ARRAY_SIZE] 	:N9
	Var voucherRef 						:A64
	Var errorMsg 						:A1024
	
	//Este control se deshabilita porque en Fast Trasaction bloquea el flujo si no se carga nada antes
	//If(@CKNUM = 0)
	//	ErrorMessage "No hay transaccion abierta"
	//	Return 
	//EndIf
	
	// Read / Input QR	
	Input qrStr, "Ingrese QR"
		
	If(@INPUTSTATUS = 0 Or Trim(qrStr) = "")
		InfoMessage "Operacion cancelada"
		Return
	EndIf
	
	Call IsQRPresent(qrStr, qrPresent)

	If(qrPresent)
		InfoMessage "Promocion ya agregada"
		Return		
	EndIf

	Call LoadQRPromotionDrv()
	
	If(gblhQRPromotionDrv = 0)
		Return
	EndIf

	DLLCall_CDecl gblhQRPromotionDrv, Validate(gblsServerIP,      gbliServerPort, qrStr, 			\
                                               Ref itemArraySize, Ref status, 						\     
                                               Ref itemType[], 	  Ref itemObjNum[],  Ref itemQty[], \
                                               1024,              Ref errorMsg,      Ref retVal) 	
	
	Call FreeQRPromotionDrv()

	If(retVal <> EXECUTION_OK)
		Call ShowErrorMessage(retVal)
		Return
	EndIf

	If(status = QR_WS_ENDPOINT_ERROR)
		ErrorMessage "Sin Comunicacion con Servicio QR"
		Return
	EndIf

	If(status = QR_FAIL)
		ErrorMessage "Fallo al procesar cupon"
		Return
	EndIf

	If(status = QR_VALID)
		
		//init promotion mark
		If(itemArraySize > 0)
			LoadKybdMacro Key(KEY_TYPE_MENU_ITEM_OBJNUM, gbliQRPromoItemObjNum), MakeKeys(qrStr), @KEY_ENTER
		EndIf
		
		Var index :N9	
		For index = 1 To itemArraySize

			If(itemType[index] = MENU_ITEM)
				LoadKybdMacro MakeKeys(itemQty[index]), Key(KEY_TYPE_MENU_ITEM_OBJNUM, itemObjNum[index]), @KEY_ENTER
			EndIf
			
			If(itemType[index] = DISCOUNT)
				LoadKybdMacro Key(KEY_TYPE_DISCOUNT_OBJNUM, itemObjNum[index]), @KEY_ENTER		
			EndIf
			
			//gbliItemObjNum[gbliItemArraySize + index] = itemObjNum[index]
			//gbliItemType[gbliItemArraySize + index] = itemType[index]
			//gbliItemArraySize = gbliItemArraySize + itemArraySize
			
		EndFor
		
		//end promotion mark
		If(itemArraySize > 0)
			LoadKybdMacro Key(KEY_TYPE_MENU_ITEM_OBJNUM, gbliQRPromoEndItemObjNum), @KEY_ENTER
		EndIf
	Else
		ErrorMessage errorMsg
	EndIf
												
EndSub

//******************************************************************
// Procedure: AddToQrCodeArray()
//******************************************************************
Sub AddToQrCodeArray(Var qrCode_ :A32, Ref qrCodeArrayCount_, Ref qrCodeArray_[], Ref qrCodeQtyArray_[])
	
	Var index :N9
	
	//1 find code in array
	For index = 1 To qrCodeArrayCount_
		If(qrCodeArray_[index] = Trim(qrCode_))				
			qrCodeQtyArray_[index] = qrCodeQtyArray_[index] + 1 
		EndIf
	EndFor
	
	//new item
	If(index > qrCodeArrayCount_)
		qrCodeArray_[index] = Trim(qrCode_)
		qrCodeQtyArray_[index] = 1
		qrCodeArrayCount_ = qrCodeArrayCount_ + 1
	EndIf

EndSub

//******************************************************************
// Procedure: RemoveOrphanVouchers()
//******************************************************************
Sub RemoveOrphanVouchers(Var cleanAll_ : N1)

	Var terminal 	:A32 = @WSID	
	Var errorMsg 	:A1024
	Var retVal 		:N1

	Call LoadQRPromotionDrv()
	
	If(gblhQRPromotionDrv = 0)
		Return
	EndIf

	//1. generate current vouchers array
	Var qrCodeArray[MAX_ITEM_ARRAY_SIZE]	:A32
	Var qrCount 							:N9

	qrCount =  0
	If(Not cleanAll_)
		Call GetQRCodeFromItem(qrCodeArray[], qrCount)
	EndIf
	
	//Optimize a little
	If(qrCount = 0 And gbliQRCodeArrayCount = 0)
		Return
	EndIf	

	//2. Consolidate them in a new array
	Var qrConsolidatedCodeArray[MAX_ITEM_ARRAY_SIZE]	:A32
	Var qrConsolidatedQty[MAX_ITEM_ARRAY_SIZE] 			:N9
	Var qrConsolidatedCount 							:N9
	Var index 											:N9
	
	For index = 1 To qrCount
		Call AddToQrCodeArray(qrCodeArray[index], qrConsolidatedCount, qrConsolidatedCodeArray[], qrConsolidatedQty[])	
	EndFor
	
	//3. Compare current consolidated array with previuos one
	//   by substracting new array from prev array
	Var prevArrayIndex 		:N9
	Var currentArrayIndex 	:N9
	
	For prevArrayIndex = 1 To gbliQRCodeArrayCount
		For currentArrayIndex = 1 To qrConsolidatedCount	
			If(gblsQRCodeArray[prevArrayIndex] = qrConsolidatedCodeArray[currentArrayIndex])
				
				gbliQRCodeQtyArray[prevArrayIndex] = gbliQRCodeQtyArray[prevArrayIndex] - qrConsolidatedQty[currentArrayIndex] 

				//free every non zero count code from DB
				For index = 1 To gbliQRCodeQtyArray[prevArrayIndex]
					DLLCall_CDecl gblhQRPromotionDrv, Void(gblsServerIP,  gbliServerPort,  gblsQRCodeArray[prevArrayIndex],  gblsStoreId, \
				                                           terminal,      1024,            Ref errorMsg,                     Ref retVal) 			
				EndFor
				Break
			EndIf					
		EndFor	
		
		If(currentArrayIndex = qrConsolidatedCount + 1)			
			DLLCall_CDecl gblhQRPromotionDrv, Void(gblsServerIP,  gbliServerPort,  gblsQRCodeArray[prevArrayIndex],  gblsStoreId, \
			                                       terminal,      1024,            Ref errorMsg,                     Ref retVal) 			
		EndIf		
	EndFor

	Call FreeQRPromotionDrv()
	
	//update last consolidated QR Code Array
	gbliQRCodeArrayCount = qrConsolidatedCount			 
	For index = 1 To gbliQRCodeArrayCount
		gblsQRCodeArray[index] = qrConsolidatedCodeArray[index]
	EndFor			
	 
EndSub

	
//******************************************************************
// Procedure: RedeemQR()
//******************************************************************
Sub RedeemQR()
	
	Var status 							:N9
	Var retVal 							:N9
	Var qrStr 							:A32
	Var voucherRef 						:A64
	Var terminal 						:A16 
	Var transactionId 					:N9	
	Var errorMsg 						:A1024
	Var voucherId 						:N9
	Var index 							:N9
	Var amount 							:$12
	Var amount100Int 					:N18
	// Get QR Code QR	
	
	Var qrCodeArray[MAX_ITEM_ARRAY_SIZE]:A32
	Var qrCount 						:N9
		
	Call GetQRCodeFromItem(qrCodeArray[], qrCount)
		
	If(qrCount = 0)
		//InfoMessage "Operacion cancelada"
		Return
	EndIf

	Call LoadQRPromotionDrv()
	
	If(gblhQRPromotionDrv = 0)
		Return
	EndIf

	Format terminal As @WSID	

	For index = 1 To qrCount
		
		//Debug
		//ErrorMessage "INDX:", index, "/", qrCount," QR:", qrCodeArray[index]
		//debug
		
		Call GetPromotionAmount(qrCodeArray[index], amount)
		amount100Int = amount * 100		
		
		Format voucherRef As "QR:", (@YEAR){02}, @MONTH{02}, @DAY{02}, @HOUR{02}, @MINUTE{02}, @WSID{04}, @CKNUM{09}

		DLLCall_CDecl gblhQRPromotionDrv, Redeem(gblsServerIP, gbliServerPort, qrCodeArray[index], voucherRef,        1024, 	   \ 
												 amount100Int, gblsStoreId,    terminal,									       \
					 							 Ref errorMsg, Ref status,     Ref voucherId,      Ref transactionId, Ref retVal) 	

		If(retVal <> EXECUTION_OK)
			Call ShowErrorMessage(retVal)
		EndIf

		If(status = QR_NOT_FOUND)
			ErrorMessage "Cupon invalido o vencido"
		EndIf

		If(status = QR_FAIL)
			ErrorMessage "Fallo al procesar cupon"
		EndIf
		
		If(status = QR_WS_ENDPOINT_ERROR)
			ErrorMessage "Sin Comunicacion con Servicio QR"
		EndIf

	EndFor

	Call FreeQRPromotionDrv()
									
EndSub

///////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                       //
//===================================== QR Promotion API FUNCTIONS ======================================//
//                                                                                                       //
///////////////////////////////////////////////////////////////////////////////////////////////////////////



///////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                       //
//========================================= AUXILIARY FUNCTIONS =========================================//
//                                                                                                       //
///////////////////////////////////////////////////////////////////////////////////////////////////////////

//******************************************************************
// Procedure: 	GetQRCodeFromItem()
// Author: 		Luis Vaccaro
//******************************************************************
Sub GetQRCodeFromItem(Ref qrCodeArray_[], Ref qrCount_)

	Var index 	:N9
	
	qrCount_ = 0
		
	For index = 1 To @NUMDTLT
		
		//debug
		//ErrorMessage "INDX:", index, "T:", @DTL_TYPE[index], " O:", @DTL_OBJECT[index], " ", @DTL_NAME[index], " QRCNT:", qrCount_ 
		//debug
    	If (@DTL_TYPE[index] = DT_MENU_ITEM And Not @DTL_IS_VOID[index] And @DTL_OBJECT[index] = gbliQRPromoItemObjNum)
        	
        	If(index + 1 <= @NUMDTLT And @DTL_TYPE[index + 1] = DT_REFERENCE)
        		qrCount_ = qrCount_ + 1
        		qrCodeArray_[qrCount_] = Trim(@DTL_NAME[index + 1])
        		
				//qrCode_ = @DTL_NAME[index + 1]
        	EndIf        		        	
		EndIf		
	EndFor 
    	
EndSub

//******************************************************************
// Procedure: 	GetPromotionAmount()
// Author: 		Luis Vaccaro
//******************************************************************
Sub GetPromotionAmount(Var promotion_ :A32, Ref amount_)

	Var index 		:N9
	Var firstIndex 	:N9 
	Var isPromotion :N1
	amount_ = 0

	//Find promotion
	
	For index = 1 To @NUMDTLT
	
		//ErrorMessage "index=", index, " TYPE=", @DTL_TYPE[index], " OBJ=", @DTL_OBJECT[index], " TYPE NEXT=", @DTL_TYPE[index + 1], " NAME NEXT='", @DTL_NAME[index + 1], "'" 
    
    	If (@DTL_TYPE[index] = DT_MENU_ITEM 				\
    		And Not @DTL_IS_VOID[index] 					\
    		And @DTL_OBJECT[index] = gbliQRPromoItemObjNum 	\ 
    		And index + 1 <= @NUMDTLT 						\
    		And @DTL_TYPE[index + 1] = DT_REFERENCE 		\
    		And Trim(@DTL_NAME[index + 1]) = promotion_)
			
			firstIndex = index + 2					
			Break				
		EndIf
	
	EndFor	

	//No promotion in check
	If(firstIndex = 0)
		Return
	EndIf
				
	For index = firstIndex To @NUMDTLT				

    	If (Not @DTL_IS_VOID[index])

	    	If (@DTL_TYPE[index] = DT_MENU_ITEM)

				//Found end Promotion, so exit
    			If (@DTL_OBJECT[index] = gbliQRPromoItemObjNum)
					Return
				EndIf
	        	
	        	//Search for Menu Item
	        	//Call BelongToPromotion(@DTL_OBJECT[index], MENU_ITEM, isPromotion)
				Call BelongToPromotion(index, isPromotion)
				
				If(isPromotion)
					amount_ = amount_ + Abs(@DTL_TTL[index])
				EndIf
					    
			ElseIf (@DTL_TYPE[index] = DT_DISCOUNT)

	        	//Search for Discout
	        	//Call BelongToPromotion(@DTL_OBJECT[index], DISCOUNT, isPromotion)
	        	Call BelongToPromotion(index, isPromotion)
				
				If(isPromotion)
					amount_ = amount_ - Abs(@DTL_TTL[index])
				EndIf
				
			EndIf		

		EndIf
	
	EndFor 

EndSub

//******************************************************************
// Procedure: 	BelongToPromotion()
// Author: 		Luis Vaccaro
//******************************************************************
//Sub BelongToPromotion(Var objNum_ :N9, Ref itemType_, Ref isPresent_)
Sub BelongToPromotion(Var index_ :N9, Ref isPresent_)
		
	Var index 	:N9

	isPresent_ = FALSE

	//search for an ending promotion item
	For index = index_ To @NUMDTLT

    	If (@DTL_TYPE[index] = DT_MENU_ITEM And Not @DTL_IS_VOID[index])
        	
        	If(@DTL_OBJECT[index] = gbliQRPromoItemObjNum)
        		Return
        	ElseIf(@DTL_OBJECT[index] = gbliQRPromoEndItemObjNum)
				isPresent_ = TRUE
        		Return
        	EndIf        		        	
		EndIf		

	EndFor
	
EndSub
//Sub BelongToPromotion(Var objNum_ :N9, Ref itemType_, Ref isPresent_)
//		
//	Var index 	:N9
//
//	isPresent_ = FALSE
//
//	For index = 1 To gbliItemArraySize
//		
//		If(gbliItemObjNum[index] = objNum_ And gbliItemType[index] = itemType_)
//			isPresent_ = TRUE
//			Return
//		EndIf		
//	EndFor
//	
//EndSub

//******************************************************************
// Procedure: 	IsQRPresent()
// Author: 		Luis Vaccaro
//******************************************************************
Sub IsQRPresent(Var qrCode_ :A32, Ref retVal_)

	Var index 		:N9
	Var qrCodeAux 	:A32 = ""

	retVal_ = FALSE
		
	For index = 1 To @NUMDTLT
		
		//debug
		//ErrorMessage "INDX:", index, "T:", @DTL_TYPE[index], " O:", @DTL_OBJECT[index], " ", @DTL_NAME[index] 
		//debug
    	
    	If (@DTL_TYPE[index] = DT_MENU_ITEM And Not @DTL_IS_VOID[index] And @DTL_OBJECT[index] = gbliQRPromoItemObjNum)
        			
			If(index + 1 <= @NUMDTLT And @DTL_TYPE[index + 1] = DT_REFERENCE)
				qrCodeAux = @DTL_NAME[index + 1]
		
				If(Trim(qrCodeAux) = Trim(qrCode_))
					retVal_ = TRUE
					Return
				EndIf						
        	EndIf        		     	
		EndIf
		
	EndFor 
    	
EndSub

//******************************************************************
// Procedure: 	ShowErrorMessage()
// Author: 		Luis Vaccaro
//******************************************************************
Sub ShowErrorMessage(Var errorCode_ :N9)
     
    Var sTmp :A80
    	
	If(errorCode_ = CONNECTION_ERROR)
		ErrorMessage "Error de conexion con REQ SERVER - QR" 
	EndIf
    		
	If(errorCode_ = PARAM_ERROR)
		ErrorMessage "Error de parametros en comando a REQ SERVER" 
	EndIf

	If(errorCode_ = TIMEOUT_ERROR)
		ErrorMessage "Error de tiempo de espera de respuesta desde REQ SERVER" 
	EndIf

	If(errorCode_ = CHECKSUM_ERROR)
		ErrorMessage "Error de checksum en mensaje desde REQ SERVER" 
	EndIf
	
	If(errorCode_ <> EXECUTION_OK)
		Format sTmp As "Error Code = ", errorCode_
		ErrorMessage sTmp 
		Call LogInfo(ERROR_LOG_FILE_NAME, sTmp, TRUE, TRUE)	
	EndIf
		
EndSub

//******************************************************************
// Procedure: 	ClearGobalVars()
// Author: 		Luis Vaccaro
//******************************************************************
Sub ClearGobalVars()	
	
	Var index :N9
	
	For index = 1 To MAX_ITEM_ARRAY_SIZE
		//gbliItemObjNum[index] = 0
		//gbliItemType[index] = 0
		//gbliItemArraySize = 0
		gblsQRCodeArray[MAX_ITEM_ARRAY_SIZE] = ""
		gbliQRCodeQtyArray[MAX_ITEM_ARRAY_SIZE] = 0
	EndFor
	
	gbliQRCodeArrayCount = 0

EndSub


//******************************************************************
// Procedure: 	SetFilePaths()
//******************************************************************
Sub SetFilePaths()
	// general paths
	If gbliWSType = PCWS_TYPE
		// This is a Win32 client
		Format PATH_TO_GEOITD_DRIVER 		As "..\bin\TWS.RES.QRClientW32.dll"		
		Format ERROR_LOG_FILE_NAME 			As "QRPromotionLog.log"
		Format CONFIGURATION_FILE_NAME 		As "QRPromotion.cfg"
		Format CONFIGURATION_FILE_NAME_PWS 	As "QRPromotion", @WSID, ".cfg"

	ElseIf gbliWSType = WS5_TYPE		
		// This is a WinCE 5.0/6.0 client		
		Format PATH_TO_GEOITD_DRIVER 		As "CF\micros\bin\TWS.RES.QRClientWCE50.dll"
		Format ERROR_LOG_FILE_NAME 			As "CF\micros\etc\QRPromotionLog.txt"
		Format CONFIGURATION_FILE_NAME 		As "CF\micros\etc\QRPromotion.cfg"
		Format CONFIGURATION_FILE_NAME_PWS 	As "CF\micros\etc\QRPromotion", @WSID, ".cfg"
	Else
		// This is a WS4 client	WinCE 4.2	
		Format PATH_TO_GEOITD_DRIVER 		As "CF\micros\bin\TWS.RES.QRClientWCE.dll"		
		Format ERROR_LOG_FILE_NAME 			As "CF\micros\etc\QRPromotionLog.txt"
		Format CONFIGURATION_FILE_NAME 		As "CF\micros\etc\QRPromotion.cfg"
		Format CONFIGURATION_FILE_NAME_PWS 	As "CF\micros\etc\QRPromotion", @WSID, ".cfg"
	EndIf
		
EndSub

//******************************************************************
// Procedure: setWorkstationType()
//******************************************************************
Sub setWorkstationType()
	
	// get RES major & minor version
	Split @VERSION, ".", gbliRESMajVer, gbliRESMinVer

	// set workstation type

	If gbliRESMinVer >= 2 or gbliRESMajVer >= 3
		gbliWSType = @WSTYPE
	Else
		// older versions don't support the
		// "@WSTYPE" system variable
		gbliWSType = 1  // PCWS as default
	EndIf
	
EndSub

//******************************************************************
// Procedure: LoadQRPromotionDrv()
//******************************************************************
Sub LoadQRPromotionDrv()

	Var retMessage :A512
	
	If (gblhQRPromotionDrv = 0)
		Call LogInfo(ERROR_LOG_FILE_NAME, "Loading QR Promotion Driver", TRUE, TRUE)
		DLLLoad gblhQRPromotionDrv, PATH_TO_GEOITD_DRIVER
    EndIf

	If gblhQRPromotionDrv = 0
		Call LogInfo(ERROR_LOG_FILE_NAME, "FAILED to load QR Promotion driver!", TRUE, TRUE)
		ErrorMessage "Failed to load QR Promotion driver!"
		Return 
    EndIf
					
EndSub

//******************************************************************
// Procedure: FreeQRPromotionDrv()
//******************************************************************
Sub FreeQRPromotionDrv()

	If gblhQRPromotionDrv <> 0
		Call LogInfo(ERROR_LOG_FILE_NAME, "Unloading QR Promotion Driver", TRUE, TRUE)	
		DLLFree gblhQRPromotionDrv
		gblhQRPromotionDrv = 0	
	EndIf

EndSub

//******************************************************************
// Procedure: 	SetCustomSetting()
// Author: 		Luis Vaccaro
// Purpose: processes the information passed by parameter,
//			classifying the type of custom setting read and 
//		    setting its value into the corresponding variable
// Parameters:
//	- sInfo_ = information string to be processed
//  - fn_	 = pointer to currently open configuration file	
//	- sFileName_ = Filename of read configuration file
//******************************************************************
Sub SetCustomSetting(Ref sInfo_, Ref fn_, Var sFileName_ : A100 )

	Var sTmp :A1000 = Trim(sInfo_)
	Var i	 :N3	// for looping

	If (Mid(sTmp,1,1) = "*")
		
		// Current line is a comment. Ignore it.
			
	ElseIf	sTmp = "STORE_ID"

		// get value (should always be found below the key)
		FReadLn fn_, sTmp
		
		// check its validity
		If Trim(sTmp) <> ""			
			gblsStoreId = sTmp			
		Else
			Call LogInfo(ERROR_LOG_FILE_NAME, "STORE_ID not specified", TRUE, TRUE)
			ErrorMessage "STORE_ID no especificado en ", sFileName_
		EndIf

	ElseIf	sTmp = "REQSERVER_SERVER_IP"

		// get value (should always be found below the key)
		FReadLn fn_, sTmp
		
		// check its validity
		If Trim(sTmp) <> ""			
			gblsServerIP = sTmp			
		Else
			Call LogInfo(ERROR_LOG_FILE_NAME, "REQSERVER_SERVER_IP not specified", TRUE, TRUE)
			ErrorMessage "REQSERVER_SERVER_IP no especificado en ", sFileName_
		EndIf

	ElseIf	sTmp = "REQSERVER_SERVER_PORT"

		// get value (should always be found below the key)
		FReadLn fn_, sTmp
		
		// check its validity
		If Trim(sTmp) <> ""					
			gbliServerPort = sTmp			
		Else
			Call logInfo(ERROR_LOG_FILE_NAME, "REQSERVER_SERVER_PORT not specified", TRUE, TRUE)
			ErrorMessage "REQSERVER_SERVER_PORT no especificado en ", sFileName_
		EndIf
								
	ElseIf	sTmp = "VOUCHER_PRINTER"

		// get value (should always be found below the key)
		FReadLn fn_, sTmp
		
		// check its validity
		If Trim(sTmp) <> ""			
			// Turn on or off Unit price flag
			gbliSelectedPrinterIndex = sTmp
			
		Else
			Call logInfo(ERROR_LOG_FILE_NAME,"VOUCHER_PRINTER not specified",TRUE,TRUE)
			ErrorMessage "VOUCHER_PRINTER not specified in ", sFileName_
		EndIf
		
	ElseIf	sTmp = "QR_PROMO_ITEM_OBJ_NUM"

		// get value (should always be found below the key)
		FReadLn fn_, sTmp
		
		// check its validity
		If Trim(sTmp) <> ""			
			// Turn on or off Unit price flag
			gbliQRPromoItemObjNum = sTmp
			
		Else
			Call logInfo(ERROR_LOG_FILE_NAME,"QR_PROMO_ITEM_OBJ_NUM not specified",TRUE,TRUE)
			ErrorMessage "QR_PROMO_ITEM_OBJ_NUM not specified in ", sFileName_
		EndIf
		
	ElseIf	sTmp = "QR_PROMO_END_ITEM_OBJ_NUM"

		// get value (should always be found below the key)
		FReadLn fn_, sTmp
		
		// check its validity
		If Trim(sTmp) <> ""			
			// Turn on or off Unit price flag
			gbliQRPromoEndItemObjNum = sTmp
			
		Else
			Call logInfo(ERROR_LOG_FILE_NAME,"QR_PROMO_ITEM_OBJ_NUM not specified",TRUE,TRUE)
			ErrorMessage "QR_PROMO_ITEM_OBJ_NUM not specified in ", sFileName_
		EndIf
		
	EndIf

EndSub

//******************************************************************
// Procedure: logInfo()
//******************************************************************
Sub LogInfo(Var sFileName_ :A100, Var sInfo_ :A3000, Var iAppend_ :N1, Var iAddTimeStamp_ :N1)

	Var fn			: N5  // file handle
	Var sTmpInfo	: A3100

	
	If iAppend_
		// append info to log file
		FOpen fn, sFileName_, append
	Else
		// overwrite existing info
		FOpen fn, sFileName_, write
	EndIf

	If fn <> 0
		
		If iAddTimeStamp_
			// add a time stamp to the record
			Format sTmpInfo As @MONTH{02}, "/", @DAY{02}, "/", (@YEAR + 2000){04}, 			\
							   " - ", @HOUR{02}, ":", @MINUTE{02}, ":", @SECOND{02}, 		\
								" | VER: ", IFC_VERSION, " | WSID: ", @WSID, " | Emp: ", 	\
							   @CKEMP, " | Chk: ", @CKNUM, " -> ", Trim(Mid(sInfo_, 1, 3000))

		Else
			// only log passed info
			Format sTmpInfo As "WSID: ", @WSID, " -> ", Trim(Mid(sInfo_, 1, 3000))

		EndIf

		// write info to log file
		FWrite fn, Trim(Mid(sTmpInfo, 1, 2048))
		
		If(Len(sTmpInfo) > 2048)
			FWrite fn, Trim(Mid(sTmpInfo, 2049, 3000))
		EndIf

		// close handle to file
		FClose fn
	Else
		// error! Warn user
		ErrorMessage "Could not log information in ", sFileName_

	EndIf

EndSub

//******************************************************************
// Procedure to PromptYesOrNo
//******************************************************************
Sub PromptYesOrNo(Ref intReturnvalue, Var strprompt : A50)
	
	Var keypress	: Key
	Var data		: A2

	ErrorBeep
	ClearIslTs
		SetIslTsKeyX  1,  1, 12, 15, 7, @KEY_ENTER, 10059, "L", 4, "Si"
		SetIslTsKeyX  1, 16, 12, 15, 7, @KEY_CLEAR, 10058, "L", 2, "No"
	DisplayIslTs

	Inputkey keypress, data, strprompt
	ClearIslTs //clear the touchscreen
	intReturnValue = keypress = @KEY_ENTER
	
EndSub

//******************************************************************
// Procedure: LoadCustomSettings()
//******************************************************************
Sub LoadCustomSettings()

	Var fn			: N5
	Var sLineInfo	: A1024
	Var sFileName	: A100
			
	// Check for WS configuration file. If it doesn't exist,
	// use "master" configuration file
	FOpen fn, CONFIGURATION_FILE_NAME_PWS, read
	If fn = 0
		FOpen fn, CONFIGURATION_FILE_NAME, Read
		Format sFileName As CONFIGURATION_FILE_NAME
	Else
		Format sFileName As CONFIGURATION_FILE_NAME_PWS
	EndIf
		
	If fn <> 0 
		While Not Feof(fn)
			FReadLn fn, sLineInfo

			// process currently read information
			Call setCustomSetting(sLineInfo,fn,sFileName)
		EndWhile

		FClose fn
	Else
		// Couldn't open configuration file!
		Call LogInfo(ERROR_LOG_FILE_NAME, "Configuration file not found", TRUE, TRUE)
		ErrorMessage "Archivo de configuracion no encontrado!"
	EndIf

EndSub
