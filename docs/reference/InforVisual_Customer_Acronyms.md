# Infor Visual Customer Acronym List

Last Updated: 2026-11-18

## What this is

- Source: Infor Visual (MTMFG) SQL Server `CUSTOMER` table, read-only.
- 882 customer records reduced to 726 unique companies plus 8 unnamed records (see appendix).
- Companies that appear under several customer IDs, plants, divisions, or spellings are collapsed to one row using the core company name.
- The `Customer ID(s)` cell lists every Infor Visual customer ID mapped to that company.
- The `Acronym` column is a unique 1-3 character code, never reused in this list.

## Usage

1. Find the company name and read its acronym.
2. Use the acronym wherever a short customer code is needed (reports, exports, filenames).
3. When a new customer is added to Infor Visual, append a row and pick an acronym not already present in this table.

## Customer acronym key

| # | Customer ID(s) | Customer name | Acronym |
| --- | --- | --- | --- |
| 1 | CID-00131-01 | A & E Hand tools | `EHT` |
| 2 | CID-00682-01 | A. Raymond Tinnerman Industrial, Inc | `RTI` |
| 3 | CID-00748-01 | ABB Installation Products | `AIP` |
| 4 | CID-00585-01 | ABL Lights | `AL` |
| 5 | CID-00176-01 | Accra-Fab | `AF` |
| 6 | CID-00346-01 | Accu Therm | `AT` |
| 7 | CID-00522-01 | Accu-Fab Inc | `AFA` |
| 8 | CID-00573-01 | Acoustic Blok | `AB` |
| 9 | CID-00368-01 | Adam Abts | `AA` |
| 10 | CID-00529-01 | AEES Inc | `AEE` |
| 11 | CID-00295-01 | AESSE Investments Ltd | `AI` |
| 12 | CID-00298-01 | AFW Foundry | `AFO` |
| 13 | CID-00631-01 | AGCO | `AGC` |
| 14 | CID-00610-01 | Agrisolutions | `AGR` |
| 15 | CID-00361-01 | Ahaus Tool and Engineering, Inc | `ATE` |
| 16 | CID-00779-01, CID-0079-01 | Aisin USA Mfg., Inc | `AIS` |
| 17 | CID-00001-01 | All-Rite Industries, Inc | `ARI` |
| 18 | CID-00220-01 | Alliance Laundry Systems LLC | `ALS` |
| 19 | CID-00575-01 | Allison Transmission | `ATR` |
| 20 | CID-00143-01 | Almco Steel | `AS` |
| 21 | CID-00757-01 | Alter Trading Corporation | `ALT` |
| 22 | CID-00155-01 | Alto Mfg. Co. Inc | `ALO` |
| 23 | CID-00288-01 | Alto-Shaam | `ASH` |
| 24 | CID-00433-01 | Alwin Manufacturing | `AM` |
| 25 | CID-00712-01 | AM General LLC | `AG` |
| 26 | CID-00419-01 | AMEREQUIP CORPORATION | `AME` |
| 27 | CID-00567-01 | AmeriStar Mfg. Inc | `AMR` |
| 28 | CID-00326-01 | Ametek | `AMT` |
| 29 | CID-00618-01 | Amphenol CIT | `AC` |
| 30 | CID-00799-01 | Anchor Manufacturing Group Inc | `AMG` |
| 31 | CIC-00409-01, CID-00409-01 | Apache Stainless Equipment Corporation | `ASE` |
| 32 | CID-00381-01 | API-THermasys | `ATH` |
| 33 | CID-00754-01 | Applied Laser Technologies | `ALA` |
| 34 | CID-00424-01 | Aprilaire, a Division of Research Products Corp | `ADR` |
| 35 | CID-00598-01 | Arc-Automotive, Inc | `AAU` |
| 36 | CID-00812-01 | Archer Wire International Corp | `AWI` |
| 37 | CID-00198-01 | Arctic Cat Inc | `ACA` |
| 38 | CID-00566-01 | Argonne National Laboratory | `ANL` |
| 39 | CID-00002-01 | Ariens Company | `ARN` |
| 40 | CID-00452-01 | ARK Models | `AMO` |
| 41 | CID-00786-01 | AROW Global Corp | `AGL` |
| 42 | CID-00649-01 | Arrival Auto | `ARA` |
| 43 | CID-00746-01 | Arrowhead Engineered Products | `AEP` |
| 44 | CID-00174-01 | Art's-Way Manufacturing | `ASW` |
| 45 | CID-00059-01 | Assembled Products | `AP` |
| 46 | CID-00570-01 | Associated Fastening Products | `AFP` |
| 47 | CID-00003-01 | Assurance Manufacturing, Inc | `AMA` |
| 48 | CID-00706-01 | AT&F Wisconsin | `AFW` |
| 49 | CID-00004-01 | ATACO Steel | `AST` |
| 50 | CID-00824-01 | ATI Forged Products- Cudahy Operations | `ATF` |
| 51 | CID-00154-01 | Atlas Metal Parts | `AMP` |
| 52 | CID-00658-01 | Atlast Machine Solutions LLC | `AMS` |
| 53 | CID-00720-01 | Attentive Industries, Inc | `AIN` |
| 54 | CID-00239-01 | Autotrol Corp | `AUT` |
| 55 | CID-00661-01 | B.E.S.T. Enterprises LLC | `BES` |
| 56 | CID-00709-01 | Badger Sheet Metal Works | `BSM` |
| 57 | CID-00440-01 | Baileigh Industrial, Inc | `BI` |
| 58 | CID-00272-01 | Baldor Dodge Reliance | `BDR` |
| 59 | CID-00609-01 | Ball Bros Machinery LTD | `BBM` |
| 60 | CID-00606-01 | Bamal Fastener Corporation/Midwest Structural Prds | `BFM` |
| 61 | CID-00611-01 | Banjo Corp | `BAN` |
| 62 | CID-00425-01 | Beaudry Electronic Motors and Controls, Inc | `BEM` |
| 63 | CID-00546-01 | Behrens Manufacturing | `BM` |
| 64 | CID-00005-01 | Benco Manufacturing | `BMA` |
| 65 | CID-00603-01 | Bendix Commercial Vehicle Sysytems LLC | `BCV` |
| 66 | CID-00549-01 | Benmar Fabrication | `BF` |
| 67 | CID-00553-01 | Biolyph | `BIO` |
| 68 | CID-00188-01 | Bitzer Scroll, Inc | `BS` |
| 69 | CID-00778-01 | Blachford RP Corporation | `BR` |
| 70 | CID-00006-01 | Bobcat Company | `BOB` |
| 71 | CID-00325-01 | Bommer Industries | `BIN` |
| 72 | CID-00160-01 | Borg-Warner Transmission Systems | `BWT` |
| 73 | CID-00129-01, CID-00353-11, CID-00789-01 | Bosch | `BOS` |
| 74 | CID-00145-01 | Boyd Northwest | `BN` |
| 75 | CID-00218-01, CID-00669-01 | Bradley Corp | `BRA` |
| 76 | CID-00211-01 | Braun Manufacturing Co., Inc | `BRM` |
| 77 | CID-00478-01 | Brembo- Brakes | `BB` |
| 78 | CID-00772-01 | Bretford Manufacturing, Inc | `BRT` |
| 79 | CID-00007-01 | Briggs & Stratton | `BST` |
| 80 | CID-00593-01 | Broadwind Heavy Fabrications | `BHF` |
| 81 | CID-00466-01, CID-00467-01 | Broan-NuTone | `BNU` |
| 82 | CID-00686-01 | Brooks Utility Products | `BUP` |
| 83 | CID-00476-01 | BRP Marine Propulsion Systems Division | `BMP` |
| 84 | CID-00673-01, CID-00814-01 | Brunner | `BRU` |
| 85 | CID-00507-01 | Bruno Independant Living Aids, Inc | `BIL` |
| 86 | CID-00351-01 | BTD Manufacturing Inc | `BTM` |
| 87 | CID-00179-01 | Buhrke Ind. LLC | `BUI` |
| 88 | CID-00635-01 | Burn Manufacturing USA LLC | `BUM` |
| 89 | CID-00672-01 | C&S Manufacturing | `CSM` |
| 90 | CID-00301-01 | C.E. Niehoff | `CEN` |
| 91 | CID-00461-01 | C3 Corporation | `C3` |
| 92 | CID-00459-01 | C4 Advanced Tactical Systems, LLC | `CAD` |
| 93 | CID-00820-01 | CAM Fasteners | `CF` |
| 94 | CID-00270-01 | Capitol Stamping | `CS` |
| 95 | CID-00277-01, CID-00785-01 | Cardinal Components Inc | `CC` |
| 96 | CID-00016-02, CID-00140-01, CID-00438-01 | Carrier | `CAR` |
| 97 | CID-00519-01 | Carter Day International Inc | `CDI` |
| 98 | CID-00017-02, CID-00320-01 | Caterpillar | `CAT` |
| 99 | CID-00481-01 | CBOL Corporation | `CBO` |
| 100 | CID-00793-01 | Celestica LLC | `CEL` |
| 101 | CID-00625-01 | Centracore, LLC | `CNT` |
| 102 | CID-00718-01 | Central City Steel | `CCS` |
| 103 | CID-00614-01 | Central Expanded Metal Inc | `CEM` |
| 104 | CID-00701-01 | Cepton | `CEP` |
| 105 | CID-00418-14 | Ceres Turf, Inc | `CT` |
| 106 | CID-00792-01 | Champion GSE | `CG` |
| 107 | CID-00316-01 | Chapter II | `CI` |
| 108 | CID-00135-01 | Chicago Heights Steel | `CHS` |
| 109 | CID-00457-01 | Chicago Tube and Iron | `CTI` |
| 110 | CID-00740-01 | Chilton Trailers Manufacturing | `CTM` |
| 111 | CID-00282-01 | Chirch Global Manufacturing, LLC | `CGM` |
| 112 | CID-00499-01 | CHIZEK TRANSPORT | `CTR` |
| 113 | CID-00436-01 | CHS Automation | `CA` |
| 114 | CID-00261-01 | Classic Coatings Inc | `CCO` |
| 115 | CID-00393-01 | CNH | `CNH` |
| 116 | CID-00568-01 | Cobham Mission Systems | `CMS` |
| 117 | CID-00828-01 | Command Engines LLC | `CE` |
| 118 | CID-00151-01 | Commercial Grounds Care, Inc | `CGC` |
| 119 | CID-00685-01 | Commscope Inc | `COM` |
| 120 | CID-00300-01 | Components Company, Inc | `CMP` |
| 121 | CID-00391-01 | Components Plus | `CP` |
| 122 | CID-00517-01 | Concealite Life Safety Products inc | `CLS` |
| 123 | CID-00012-01 | Conn-Selmer Inc | `CSE` |
| 124 | CID-00263-01 | Consolidated Metco. Inc | `CM` |
| 125 | CID-00286-01 | Consumers, Inc | `CON` |
| 126 | CID-00241-08, CID-00415-01 | Continental Industries Inc | `CIN` |
| 127 | CID-00293-01 | Converteam | `CNV` |
| 128 | CID-00737-01 | Cosby's LTD | `COS` |
| 129 | CID-00385-01 | Cree Lighting USA LLC | `CL` |
| 130 | CID-00417-01 | Crenlo | `CRE` |
| 131 | CID-00643-01, CID-00829-01 | Cretex Specialty Products | `CSP` |
| 132 | CID-00576-01 | Crystal Finishing Systems, Inc | `CFS` |
| 133 | CID-00407-01 | CTS Corporation | `CTS` |
| 134 | CID-00133-01 | Cummings Filtrations Inc | `CFI` |
| 135 | CID-00555-01 | Cummins | `CUM` |
| 136 | CID-00544-01 | Curt Group | `CGR` |
| 137 | CID-00162-01 | Cutting Edge Industries | `CEI` |
| 138 | CID-00693-01 | CVG | `CVG` |
| 139 | CID-00229-01 | CYBEX | `CYB` |
| 140 | CID-00013-01 | D & D Instruments | `DDI` |
| 141 | CID-00201-01 | D&H Industries | `DHI` |
| 142 | CID-00015-01, CID-00015-02, CID-00015-03, CID-00015-04, CID-00015-05, CID-00015-06, CID-00015-07, CID-00015-08, CID-00015-09, CID-00015-10, CID-00015-11, CID-00015-12, CID-00015-13, CID-00015-14, CID-00015-15, CID-00015-16, CID-00015-17, CID-00015-18, CID-00015-19, CID-00015-21, CID-00015-22, CID-00015-23, CID-00015-24, CID-00015-25, CID-00015-26, CID-00015-27, CID-00015-28, CID-00015-29, CID-00015-30, CID-00015-31, CID-00015-34, CID-00015-58, CID-00015-65 | Daimler Trucks North America | `DTN` |
| 143 | CID-00730-01 | Dakkota | `DAK` |
| 144 | CID-00511-01 | Dalsin Industries Inc | `DI` |
| 145 | CID-00165-01 | David L. Rasmussen | `DLR` |
| 146 | CID-00691-01 | Davinci Engineering | `DE` |
| 147 | CID-00152-01 | Dayton Rogers | `DR` |
| 148 | CID-00805-01 | DCI International | `DIN` |
| 149 | CID-00429-01 | DCI- Artform | `DA` |
| 150 | CID-00493-01 | Deco Products | `DP` |
| 151 | CID-00823-01 | Dedienne Aerospace Miami | `DAM` |
| 152 | CID-00537-01 | Dee Zee Mfg | `DZ` |
| 153 | CID-00460-01 | Denso MAnufacturing Michigan, Inc | `DMM` |
| 154 | CID-00798-01 | DESCO Industries Incorporated | `DEI` |
| 155 | CID-00015-66, CID-00761-01 | Detroit Diesel | `DD` |
| 156 | CID-00210-01 | Dielectric Corp | `DIE` |
| 157 | CID-00716-01, CID-00716-02 | Discover Energy | `DSE` |
| 158 | CID-00498-01 | Diverse Tooling Inc | `DT` |
| 159 | CID-00008-01 | Diversified Assembly Technologies | `DAT` |
| 160 | CID-00287-01 | Diversified Machine, Inc | `DM` |
| 161 | CID-00252-01 | Dixie-Narco Inc | `DN` |
| 162 | CID-00302-01 | Dixien LLC | `DIX` |
| 163 | CID-00158-01, CID-00659-01 | Donaldson | `DON` |
| 164 | CID-00430-01 | Doosan Infracore Portable Power | `DIP` |
| 165 | CID-00373-01 | Douglas Autotech Corporation | `DAU` |
| 166 | CID-00014-01 | Dowco | `DOW` |
| 167 | CID-00329-03, CID-00767-01 | DR Power | `DPO` |
| 168 | CID-00508-01 | DSSI | `DSS` |
| 169 | CID-00058-01 | Dubuque Stamping & Mfg., Inc | `DS` |
| 170 | CID-00291-01 | Dura Automotive Systems, Inc | `DAS` |
| 171 | CID-00760-01 | Dynamic Manufacturing Inc | `DMA` |
| 172 | CID-00319-01 | Dynax America Corporation | `DYA` |
| 173 | CID-00723-01 | E.C. Styberg Engineering | `ECS` |
| 174 | CID-00434-01 | E.K. Machine | `EKM` |
| 175 | CID-00422-01 | E.R. Wagner Manufacturing | `ERW` |
| 176 | CID-00471-01 | E91 | `E91` |
| 177 | CID-00222-01 | Eagle Wings Ind | `EWI` |
| 178 | CID-00161-01 | East Moline Metal Products Co | `EMM` |
| 179 | CID-00688-01 | Easy Up Inc | `EU` |
| 180 | CID-00563-01 | Eaton Corporation | `EAT` |
| 181 | CID-00225-01 | Eberspaecher Climate Control Systems | `ECC` |
| 182 | CID-00448-01 | Echo Incorporated | `ECH` |
| 183 | CID-00468-01 | Edstrom Industries, LLC | `EI` |
| 184 | CID-00612-01 | Electrolux Major Appliances - Global Purchasing | `EMA` |
| 185 | CID-00624-01 | Electronic Theatre Controls, Inc | `ETC` |
| 186 | CID-00355-01 | Elkay Manufacturing Company | `EM` |
| 187 | CID-00794-01 | EmberGlo | `EMB` |
| 188 | CID-00601-01 | Emerson Commercial & Residential Solutions | `ECR` |
| 189 | CID-00389-01 | EMTEQ Inc | `EMT` |
| 190 | CID-00734-01 | Endries International Inc | `EIN` |
| 191 | CID-00408-01 | Energy Bank, Inc | `EB` |
| 192 | CID-00510-01 | Enerpac | `ENE` |
| 193 | CID -00825-01 | Enphase Energy | `EE` |
| 194 | CID-00395-01 | Entegee | `ENT` |
| 195 | CID-00497-01 | Euro-Group | `EG` |
| 196 | CID-00569-01 | Evapco, Inc | `EVA` |
| 197 | CID-00398-01 | Excel Injection Molding | `EIM` |
| 198 | CID-00305-01 | FAB SUPPLY INC | `FS` |
| 199 | CID-00809-01 | Fall River Die Cast | `FRD` |
| 200 | CID-00781-01 | Fameccanica North America, Inc | `FNA` |
| 201 | CID-00541-01 | Fast-Rite International | `FRI` |
| 202 | CID-00451-01, CID-00451-02 | FASTENAL Company | `FAS` |
| 203 | CID-00447-01, CID-00447-02 | Faurecia | `FAU` |
| 204 | CID-00335-01, CID00335-01 | Fawn Manufacturing | `FM` |
| 205 | CID-00256-01, CID-00256-02, CID-00680-01 | Federal-Mogul | `FED` |
| 206 | CID-00613-01 | Fellowes Brands | `FB` |
| 207 | CID-00390-01 | FEMA Corporation of Michigan, Inc | `FMI` |
| 208 | CID-00777-01 | FiberTec Laser Cutting | `FLC` |
| 209 | CID-00694-01 | Fikst | `FIK` |
| 210 | CID-00144-01 | Filtertek Inc | `FIL` |
| 211 | CID-00707-01 | First Solar, Inc | `FSO` |
| 212 | CID-00207-01, CID-00207-02, CID-00207-03 | Fisher Barton | `FIB` |
| 213 | CID-000177-01 | Fisher Hamilton | `FH` |
| 214 | CID-00412-01 | Fiskars Brands- Americas | `FBA` |
| 215 | CID-00296-01 | Flambeau Inc | `FLA` |
| 216 | CID-00396-01 | FLATIRON Crane Operating Company LLC | `FCO` |
| 217 | CID-00494-01 | Flex | `FLE` |
| 218 | CID-00142-01 | Flex-N-Gate | `FNG` |
| 219 | CID-00599-01 | Flexitech Group | `FG` |
| 220 | CID-00163-01 | Flexsteel Industries | `FI` |
| 221 | CID-00458-01 | Flextronics International Europe BV | `FIE` |
| 222 | CID-00652-01 | Flowserve | `FLO` |
| 223 | CID-00435-01 | Fontaine Modification Company | `FMO` |
| 224 | CID-00364-01 | Formrite | `FOR` |
| 225 | CID-00177-01 | Foster Needle Co., INC | `FN` |
| 226 | CID-00219-01 | Freudenberg-NOK | `FNO` |
| 227 | CID-00695-01 | Furey Filter & Pump, Inc | `FFP` |
| 228 | CID-00427-01 | Furnel Inc | `FUR` |
| 229 | CID-00562-01 | G & R Elevators Mfg. Inc | `GRE` |
| 230 | CID-00743-01 | G Lighting | `GL` |
| 231 | CID-00564-01 | Gallagher Corporation | `GAL` |
| 232 | CID-00803-01 | Gamber-Johnson | `GJ` |
| 233 | CID-00016-01 | Gaterman MFG. Co., Inc | `GAT` |
| 234 | CID-00589-01 | GBSA | `GBS` |
| 235 | CID-00797-01 | GE Appliances | `GA` |
| 236 | CID-00017-01 | GE-Oil & Gas Operations, LLC | `GOG` |
| 237 | CID-00205-01 | Gehl Company | `GEH` |
| 238 | CID-00329-01, CID-00329-02, CID-00329-04 | Generac | `GEN` |
| 239 | CID-00182-01 | General Press & Fabricating | `GPF` |
| 240 | CID-00782-01 | Girbau Industrial Inc | `GI` |
| 241 | CID-00195-01 | GITS MFG. Co | `GIT` |
| 242 | CID-00475-01 | GKN Sinter Metals | `GSM` |
| 243 | CID-00186-01 | GKN Walterseid Inc | `GW` |
| 244 | CID-00818-01 | Global Power Components | `GPC` |
| 245 | CID-00146-01 | GMP Metal Products | `GMP` |
| 246 | CID-00297-01 | Gold Engines, LLC | `GE` |
| 247 | CID-00392-01 | Gordon Aluminum Ind | `GAI` |
| 248 | CID-00464-01 | Graco Inc | `GRA` |
| 249 | CID-00107-01 | Graham-Field Health Products | `GFH` |
| 250 | CID-00654-01 | Grassland Stamping Solutions | `GSS` |
| 251 | CID-00251-01, CID-00251-02 | Grede Foundries, Inc | `GF` |
| 252 | CID-00766-01 | Green Industrial Supply, Inc | `GIS` |
| 253 | CID-00744-01 | Greenbrier Central - St. Charles | `GCS` |
| 254 | CID-00571-01 | Greenheck Fan Corporation | `GFA` |
| 255 | CID-00246-01 | Greenwood Fabricating & Plating LLC | `GFP` |
| 256 | CID-00623-01 | H.E. Williams | `HEW` |
| 257 | CID-00278-01 | H.O. Bostrom | `HOB` |
| 258 | CID-00547-01 | Hadley Products | `HP` |
| 259 | CID-00535-01 | Hamann Construction Company | `HC` |
| 260 | CID-00501-01 | Hamilton Laboratory Solutions, LLC | `HLS` |
| 261 | CID-00530-01 | Hamlet & Smith | `HS` |
| 262 | CID-00421-01 | Hardscape Pro LLC | `HPR` |
| 263 | CID-00725-01, CID-00727-01 | Harper Trucks, Inc | `HT` |
| 264 | CID-00410-01 | Harris Manufacturing Inc | `HM` |
| 265 | CID-00166-01 | Hatco Corporation | `HAT` |
| 266 | CID-00311-01 | Hayes Bicycle | `HB` |
| 267 | CID-00402-01 | HB Performance Systems, Inc | `HPS` |
| 268 | CID-00168-01 | Headly Manufacturing | `HMA` |
| 269 | CID-00596-01 | Helander Metal | `HME` |
| 270 | CID-00289-01 | Helio Precision, Inc | `HEL` |
| 271 | CID00337-01 | Henderson Products, INC | `HND` |
| 272 | CID-00303-01 | Henning Inc | `HNN` |
| 273 | CID-00206-01, CID-00206-03, CID-00206-04 | Heraeus Electro-Nite | `HEN` |
| 274 | CID-00345-01 | Highway Equipment Company | `HE` |
| 275 | CID-00523-01 | Hilti North America | `HNA` |
| 276 | CID-00271-01 | HITECH, INC | `HIT` |
| 277 | CID-00181-01 | HK Systems, Inc | `HSY` |
| 278 | CID-00708-01 | HLI Solutions, Inc | `HSO` |
| 279 | CID-00209-01 | HMF Innovations | `HI` |
| 280 | CID-00764-01 | HON Organization | `HO` |
| 281 | CID-00403-01 | Honda Power Equipment Mfg. Inc | `HPE` |
| 282 | CID-00473-01 | Horton, Inc | `HOR` |
| 283 | CID-00783-01 | Howl Campfires | `HCA` |
| 284 | CID-00763-01 | Howmet Aerospace | `HA` |
| 285 | CID-00719-01 | Hubbell Electrical Products | `HEP` |
| 286 | CID-00192-01 | Husqvarna Outdoor Products | `HOP` |
| 287 | CID-00784-01 | Hussey Seating | `HSE` |
| 288 | CID-00600-01 | Hustler Conveyor Co | `HCO` |
| 289 | CID-00592-01 | Hutchinson Aerospace & Industry, Inc | `HAI` |
| 290 | CID-00642-01 | Ice Industries Grenada | `IIG` |
| 291 | CID-00543-01 | IDEAL INDUSTRIES LIGHTING | `IIL` |
| 292 | CID-00443-01 | IDP Incorporated | `IDP` |
| 293 | CID-00536-01, CID-00795-01 | IEA | `IEA` |
| 294 | CID-00627-01 | IMH Products | `IP` |
| 295 | CID-00578-01, CID-00590-01, CID-00651-01 | IMMI | `IMM` |
| 296 | CID-00362-01 | Imperial Group, LP | `IG` |
| 297 | CID-00018-01 | In-Sink-Erator | `ISE` |
| 298 | CID-00399-01 | Ingersoll Rand | `IR` |
| 299 | CID-00477-01 | Insulation Specialties of America | `ISA` |
| 300 | CID-00515-01 | International Equipment Solutions | `IES` |
| 301 | CID-00791-01 | Inventive-Group | `IGR` |
| 302 | CID-00265-01 | Invincible Office Furniture | `IOF` |
| 303 | CID-00714-01 | ITC Manufacturing | `IM` |
| 304 | CID-00387-01 | ITW Ark-Les Custom Products | `IAL` |
| 305 | CID-00189-01 | ITW Shakeproof | `IS` |
| 306 | CID-00539-01 | J.J. Keller & Associates | `JJK` |
| 307 | CID-00184-01 | J.L. French Automotive Castings | `JLF` |
| 308 | CID-00020-01 | Jagemann Stamping Company | `JS` |
| 309 | CID-00591-01 | JAS Alloy, LLC | `JA` |
| 310 | CID-00193-01 | JD Norman Industries | `JNI` |
| 311 | CID-00021-01 | Jeninga Bros. Metal Forming Inc | `JBM` |
| 312 | CID-00790-01 | JM Steel | `JST` |
| 313 | CID-00064-01 | Joerns Healthcare Inc | `JH` |
| 314 | CID-00386-01 | John Crane Orion | `JCO` |
| 315 | CID-00022-01, CID-00022-02, CID-00022-03, CID-00022-04, CID-00022-05, CID-00022-06, CID-00022-07, CID-00022-08, CID-00022-09, CID-00022-10, CID-00022-11, CID-00022-12, CID-00022-13, CID-00022-14, CID-00022-15, CID-00022-16, CID-00022-17, CID-00199-01, CID-00574-01, CID-00579-01 | John Deere | `JD` |
| 316 | CID-00533-01 | John's Industries | `JSI` |
| 317 | CID-00526-01 | Johnson Controls | `JC` |
| 318 | CID-00634-01 | Jorgensen Conveyor Inc | `JOC` |
| 319 | CID-00826-01 | Julian Electric Inc | `JE` |
| 320 | CID-00248-01 | Just In Time Corporation | `JIT` |
| 321 | CID-00413-01 | Justright Manufacturing Company LLC | `JM` |
| 322 | CID-00738-01 | K-Hart Industries Ltd | `KHI` |
| 323 | CID-00216-01 | K-TEK Corp | `KT` |
| 324 | CID-00735-01 | Kalas Wire | `KW` |
| 325 | CID-00366-01 | Kapco Inc | `KAP` |
| 326 | CID-00167-01 | Karavan Trailers, Inc | `KTR` |
| 327 | CID00336-01 | Karl Schmidt Unisia | `KSU` |
| 328 | CID-00312-01 | Kaufman Mfg. Co | `KAU` |
| 329 | CID-00331-01, CID-00331-02, CID-00559-01 | Kawasaki Motors Mfg. Corp | `KM` |
| 330 | CID-00259-01 | Keats Manufacturing Co | `KMA` |
| 331 | CID-00052-01, CID-00294-01 | Kees | `KEE` |
| 332 | CID-00664-01 | Kelly Design & Development | `KDD` |
| 333 | CID-00665-01 | Kenosha Lumber | `KL` |
| 334 | CID-00830-01 | KettenWulf Inc | `KET` |
| 335 | CID-00726-01 | Kewaunee Fabrications | `KF` |
| 336 | CID-00492-01 | Key Tronic EMS | `KTE` |
| 337 | CID-00405-01 | Keystone Sensors LLC | `KS` |
| 338 | CID-00273-01 | Kimberly Clark | `KC` |
| 339 | CID-00400-01 | KMC Stampings | `KST` |
| 340 | CID-00262-01 | Knaack | `KNA` |
| 341 | CID-00374-01 | KNM, LLC | `KNM` |
| 342 | CID-00604-01 | Kobelco Construction Machinery U.S.A. Inc | `KCM` |
| 343 | CID-00023-01, CID-00660-01, CID-00660-02, CID-00660-03, CID-00756-01 | Kohler | `KOH` |
| 344 | CID-00110-01 | Kondex Corporation | `KON` |
| 345 | CID-00269-01 | Koss Corporation | `KOS` |
| 346 | CID-00770-01 | Kranze Technology Solutions | `KTS` |
| 347 | CID-00787-01 | Kreider Sourcing LLC | `KSO` |
| 348 | CID-00024-01, CID-00024-02, CID-00024-03, CID-00024-04 | Krueger International | `KI` |
| 349 | CID-00595-01, CID-00677-01 | Kruger | `KRU` |
| 350 | CID-00538-01 | Kysor Warren EPTA US Corporation | `KWE` |
| 351 | CID00340-01 | L.E. Johnson | `LEJ` |
| 352 | CID-00811-01 | Lake Air Products | `LAP` |
| 353 | CID-00487-01, CID-00622-01 | Lake Shore Systems | `LSS` |
| 354 | CID-00397-01 | LAMCO Sling & Rigging | `LSR` |
| 355 | CID-00552-01 | Landmark Manufacturing | `LM` |
| 356 | CID-00260-01 | Lapham-Hickey Steel | `LHS` |
| 357 | CID-00359-01 | Laser Technologies Inc | `LT` |
| 358 | CID-00561-01 | Lavelle Industries, Inc | `LI` |
| 359 | CID-00057-01, CID-00420-01 | LDI Industries | `LDI` |
| 360 | CID-00801-01 | Legrand | `LEG` |
| 361 | CID-00245-01, CID-00245-02, CID-00607-01 | Lennox | `LEN` |
| 362 | CID-00699-01 | Lewis Engineering Co | `LE` |
| 363 | CID-00749-01 | LG Electronics | `LEL` |
| 364 | CID-00046-01 | Liberty Steel Products, Inc | `LSP` |
| 365 | CID00341-01 | Lincoln Electric CO | `LIE` |
| 366 | CID-00637-01 | Lincoln Industries | `LII` |
| 367 | CID-00815-01 | LinmoreLED | `LIN` |
| 368 | CID-00495-01, CID-00813-01 | Lippert | `LIP` |
| 369 | CID-00656-01 | Lovesac | `LOV` |
| 370 | CID-00771-01 | Lozier Corporation | `LOZ` |
| 371 | CID-00150-01 | Luitink Mfg. Co | `LUI` |
| 372 | CID-00641-01 | Lyco Mfg | `LYC` |
| 373 | CID-00474-01 | Lyko Steel LLC | `LS` |
| 374 | CID-00788-01 | M&M Manufacturing | `MMM` |
| 375 | CID-00700-01 | M-B Companies | `MBC` |
| 376 | CID-00780-01 | MAASS Midwest Inc | `MM` |
| 377 | CID-00060-01 | Mac-Tech | `MT` |
| 378 | CID-00444-01, CID-00472-01, CID-00472-02 | MAHLE | `MAH` |
| 379 | CID-00442-01 | Manitou North Americas LLC | `MNA` |
| 380 | CID-00378-01 | Manitowoc Cranes, LLC | `MC` |
| 381 | CID-00062-01 | Manitowoc Grey Iron Foundry, Inc | `MGI` |
| 382 | CID-00339-01 | Manitowoc Heating | `MH` |
| 383 | CID-00026-01 | Manitowoc Relay & Protectors, Inc | `MRP` |
| 384 | CID-00027-01 | Manitowoc Tool & Machining | `MTM` |
| 385 | CID-00290-01 | Mantz Automation | `MA` |
| 386 | CID-00306-01, CID-00733-01 | Marion Body Works, Inc | `MBW` |
| 387 | CID-00644-01 | Mark One Manufacturing | `MOM` |
| 388 | CID-00379-01 | Marvel Manufacturing | `MMA` |
| 389 | CID-00489-01 | Massiv Die-Form | `MDF` |
| 390 | CID-00237-01 | Master Lock Company LLC | `ML` |
| 391 | CID-00581-01 | Matech Solutions | `MS` |
| 392 | CID-00406-01 | Material Handling Innovations | `MHI` |
| 393 | CID-00729-01 | Mathison Manufacturing Inc | `MAM` |
| 394 | CID-00423-01 | Matrinrea Industrial Mexico | `MIM` |
| 395 | CID-00465-01 | Mayline | `MAY` |
| 396 | CID-00217-01 | Maysteel LLC | `MST` |
| 397 | CID-00204-01 | Mayville Engineering Company, Inc | `ME` |
| 398 | CID-00807-01 | McFarlane Manufacturing | `MCM` |
| 399 | CID-00344-01 | McGregor Metalworking Company | `MME` |
| 400 | CID-00183-01 | McKEY Perforating Co. Inc | `MP` |
| 401 | CID-00242-08 | McNeilus Companies Inc | `MCO` |
| 402 | CID-00318-01 | Medalist Laserfab | `MLA` |
| 403 | CID-00666-01 | Medalist Manufacturing Inc | `MEM` |
| 404 | CID-00343-01 | Megtec Systems | `MSY` |
| 405 | CID-00015-67 | Mercedes-Benz Res. & Dev. India Pvt. Ltd | `MBR` |
| 406 | CID-00067-03 | Mercruiser | `MER` |
| 407 | CID-00048-01 | Mercury Aircraft Inc | `MAI` |
| 408 | CID-00310-01 | Mercury Castings | `MCA` |
| 409 | CID-00136-01 | Mercury Marine | `MRC` |
| 410 | CID-00520-01 | Meritor | `MRT` |
| 411 | CID-00540-01 | Metal Art of Wisconsin | `MAW` |
| 412 | CID-00313-01 | Metal Marketers | `MET` |
| 413 | CID-00322-01 | Metal masters/MC2 Sales | `MMS` |
| 414 | CID-00360-01 | MetalCraft of Mayville | `MTL` |
| 415 | CID-00187-01 | Metalex | `MEA` |
| 416 | CID-00831-01 | Metalfx | `MEL` |
| 417 | CID-00741-01 | Metallics Inc | `MEI` |
| 418 | CID-00800-01, CID-00822-01 | Metals USA | `MES` |
| 419 | CID-00572-01 | Metko Inc | `MTK` |
| 420 | CID-00647-01 | Michelin | `MIC` |
| 421 | CID-00545-01 | Mid-Continent Instruments and Avionics | `MCI` |
| 422 | CID-00172-01 | Mid-States Aluminum Corp | `MSA` |
| 423 | CID-00692-01 | Mid-states Bolt & Screw Co | `MSB` |
| 424 | CID-00724-01 | Midco International, Inc | `MI` |
| 425 | CID-00171-01 | Middle Atlantic Products, Inc | `MAP` |
| 426 | CID-00072-01 | Middleby Cooking Systems Group | `MCS` |
| 427 | CID-00061-01 | Midwest Die Casting | `MDC` |
| 428 | CID-00350-01 | Midwest Wire Products LLC | `MWP` |
| 429 | CID-00028-01, CID-00028-02 | Miller Electric | `MLE` |
| 430 | CID-00445-01 | Miller Felpax Corp | `MF` |
| 431 | CID-00029-01 | Miller St. Nazianz | `MSN` |
| 432 | CID-00662-01 | Milsco | `MLS` |
| 433 | CID-00382-01, CID-00630-01 | Milwaukee Tool | `MIL` |
| 434 | CID-00206-02 | Minco Group | `MG` |
| 435 | CID-00274-01 | Minneapolis Washer & Stamping, Inc | `MWS` |
| 436 | CID-00462-01 | MIRO MANUFACTURING INC | `MIR` |
| 437 | CID-00504-01 | Mitsubishi Fuso | `MFU` |
| 438 | CID-00203-01 | MJC Engineering and Technology, Inc | `MEN` |
| 439 | CID-00437-01, CID-00534-01 | Modern Tool Inc | `MTO` |
| 440 | CID-00235-01 | Modine Manufacturing Company | `MO` |
| 441 | CID-00565-01 | ModTruss | `MOD` |
| 442 | CID-00750-01 | Molded Dimensions Group | `MDG` |
| 443 | CID-00030-01, CID-00375-01 | Molded Fiber Glass | `MFG` |
| 444 | CID-00626-01 | Monroe | `MON` |
| 445 | CID-00079-01 | Morris Material Handling | `MMH` |
| 446 | CID-00307-01 | Morrison Bros. Co | `MB` |
| 447 | CID-00456-01 | MORryde International, Inc | `MIN` |
| 448 | CID-00470-01 | Morton Industries LLC | `MOI` |
| 449 | CID-00479-01 | Mountain Manufacturing | `MOU` |
| 450 | CID-00676-01 | MPE Inc | `MPE` |
| 451 | CID-00776-01 | Muza Metal Products | `MMP` |
| 452 | CID-00214-01 | NACCO Materials Handling Group, Inc | `NMH` |
| 453 | CID-00636-01 | Nahanni Steel Products Inc | `NSP` |
| 454 | CID-00796-01 | Nailor Industries | `NI` |
| 455 | CID-00518-01 | NAPCO International, LLC | `NIN` |
| 456 | CID-00404-01 | National Electric Coil | `NEC` |
| 457 | CID-00208-01, CID-00455-01 | National Metalwares, LP | `NM` |
| 458 | CID-00670-01 | Nautilus, Inc | `NAU` |
| 459 | CID-00081-01 | Neenah Foundry Company | `NF` |
| 460 | CID-00742-01 | Nelson Global Products | `NGP` |
| 461 | CID-00416-01, CID-00449-01 | Nemak | `NEM` |
| 462 | CID-00617-01 | NEOTech | `NEO` |
| 463 | CID-00556-01 | New Standard | `NS` |
| 464 | CID-00584-01 | Nexteer Automotive | `NA` |
| 465 | CID-00247-01 | Nextera Energy | `NE` |
| 466 | CID-00758-01 | Nextracker | `NEX` |
| 467 | CID-00524-01 | Nicolet Plastics | `NP` |
| 468 | CID-00605-01 | nLight | `NLI` |
| 469 | CID-0011-01, CID-00648-01 | NN Inc | `NN` |
| 470 | CID-00594-01 | Noblitt Fabricating | `NFA` |
| 471 | CID-00775-01 | Norco Mfg. Corp | `NOR` |
| 472 | CID-00083-01 | Nordco | `NRD` |
| 473 | CID-00031-01 | Norlen Inc | `NRL` |
| 474 | CID-00689-01 | Norstar Aluminum Molds Inc | `NAM` |
| 475 | CID-00597-01 | North American Lighting | `NAL` |
| 476 | CID-00264-01 | Northern Wire, LLC | `NW` |
| 477 | CID-00340-01, CID-00483-01 | Northport / MirroCraft | `NMI` |
| 478 | CID-00755-01 | Northstar Metal Products | `NMP` |
| 479 | CID-00615-01, CID-00621-01 | NTN | `NTN` |
| 480 | CID-00560-01 | nVent | `NVE` |
| 481 | CID-00255-01 | ODM Tool & Manufacturing Co | `OTM` |
| 482 | CID-00226-01 | Oil-Rite Corporation | `OR` |
| 483 | CID-00032-01 | Oldenburg Group Incorporated | `OG` |
| 484 | CID-00105-01 | Olson International | `OI` |
| 485 | CID-00528-01 | On Target Machine | `OTA` |
| 486 | CID-00825-01 | One Monroe | `OM` |
| 487 | CID-00342-01 | Orchid Monroe,LLC | `OMO` |
| 488 | CID-00227-01, CID-00228-01 | Orion Energy Systems | `OES` |
| 489 | CID-00674-01 | Osborn | `OSB` |
| 490 | CID-00280-01 | Oshkosh Corp | `OSH` |
| 491 | CID-00554-01 | OSM Engineering | `OE` |
| 492 | CID-00629-01 | OSS Suppressors LLC | `OS` |
| 493 | CID-00191-01 | P&G Steel Products Co., Inc | `PGS` |
| 494 | CID-00702-01, CID-00717-01 | P&P Industries | `PPI` |
| 495 | CID-00808-01 | PACCAR | `PAC` |
| 496 | CID-00170-01 | Palmer Hamilton | `PH` |
| 497 | CID-00088-01 | Paper Converting Machine Company | `PCM` |
| 498 | CID-00683-01 | Paramont Manufacturing LLC | `PM` |
| 499 | CID-00141-01, CID-00141-02, CID-00141-03 | Parker Hannifin | `PAR` |
| 500 | CID-00200-01 | Parkview Metal Products | `PMP` |
| 501 | CID-00223-01 | Payson Casters | `PC` |
| 502 | CID-00667-01 | Pella Windows and Doors | `PWD` |
| 503 | CID-00678-01 | Pennant | `PEN` |
| 504 | CID-00025-01, CID-00338-01, CID-00377-01, CID-00480-01 | Pentair | `PNT` |
| 505 | CID-00180-01 | Perfection Spring & Stamping Corp | `PSS` |
| 506 | CID-00369-01 | Performance Stamping | `PS` |
| 507 | CID-00372-01 | Perlick Corporation | `PER` |
| 508 | CID-00285-01 | Peterson Mfg | `PET` |
| 509 | CID-00190-01 | PFERD Advanced Brush | `PAB` |
| 510 | CID-00739-01 | Phase Technologies | `PT` |
| 511 | CID-00194-01 | Pierce Manufacturing Inc | `PMA` |
| 512 | CID-00380-01 | Plainfield Precision | `PP` |
| 513 | CID-00011-01 | Plainfield Stamping, Inc | `PST` |
| 514 | CID-00454-01 | Plexus Manufacturing Solutions | `PMS` |
| 515 | CID-00732-01 | Plymouth Industries, Inc | `PI` |
| 516 | CID-00299-01 | Polar Ware/ Stoelting | `PWS` |
| 517 | CID-00033-01 | Polaris Industries, Inc | `PIN` |
| 518 | CID-00363-01 | Polyfab Corp | `POL` |
| 519 | CID-00384-01 | Power Packer | `PPA` |
| 520 | CID-00486-01 | Poynette Ironworks | `PIR` |
| 521 | CID-00488-01 | Precision Metal Industries | `PMI` |
| 522 | CID-00426-01 | Precision Paragon | `PRP` |
| 523 | CID-00267-01 | Precision Stamping Products | `PSP` |
| 524 | CID-00698-01 | Premier Aluminum, LLC | `PA` |
| 525 | CID-00640-01 | Premier Tooling & Manufacturing, LLC | `PTM` |
| 526 | CID-00138-01, CID-00324-01 | Prestige Threaded Products | `PTP` |
| 527 | CID-00645-01 | Presto Geosystems | `PG` |
| 528 | CID-00832-01 | Price Industries Incorporated | `PRI` |
| 529 | CID-00446-01 | Prince Castle | `PCA` |
| 530 | CID-00173-01 | Principle Manufacturing Corporation | `PRM` |
| 531 | CID-00752-01 | Processed Metals Innovators | `PME` |
| 532 | CID-00153-01 | Processing & Automation | `PAU` |
| 533 | CID-00827-01 | Professional Plating, Inc | `PPL` |
| 534 | CID-00414-01, CID00324-01 | Professional Power Products | `PPP` |
| 535 | CID-00639-01 | Pulverman | `PUL` |
| 536 | CID-00276-01 | Purem Novi Inc | `PN` |
| 537 | CID-00284-01 | Putzmeister America, Inc | `PAM` |
| 538 | CID-00236-01 | Racine Federated, Inc | `RF` |
| 539 | CID-00147-01 | Racine Stamping | `RS` |
| 540 | CID-00774-01 | Radar | `RAD` |
| 541 | CID-00091-01, CID-00431-01, CID-00439-01 | Rafael | `RAF` |
| 542 | CID-00258-01 | Ramcel Engineering | `RE` |
| 543 | CID-00632-01 | Raybestos Powertrain, LLC | `RP` |
| 544 | CID-00336-01 | Raynor Garage Doors | `RGD` |
| 545 | CID-00679-01 | Redline Plastics | `RPL` |
| 546 | CID-00334-01, CID-00337-01, CID00334-01 | Regal Beloit | `RB` |
| 547 | CID-00663-01 | Remark Technologies, Inc | `RT` |
| 548 | CID-00675-01 | Renegade Parts Washers and Detergents | `RPW` |
| 549 | CID-00371-01 | Rentapen Inc | `REN` |
| 550 | CID-00283-01, CID-00817-01 | RES Manufacturing Co | `RM` |
| 551 | CID-00232-01 | Rexnord Coupling Group | `RCG` |
| 552 | CID-00213-01 | Reynolds Presto Products Inc | `RPP` |
| 553 | CID-00697-01 | Rheem Manufacturing Co | `RMA` |
| 554 | Cid-00557-01 | Rimage Corporation | `RIM` |
| 555 | CID-00690-01 | Ritchie Industries Inc | `RI` |
| 556 | CID-00164-01 | Robbins Manufacturing, Inc | `ROM` |
| 557 | CID-00710-01 | Robinson Inc | `ROB` |
| 558 | CID-00106-01 | Rockford Process Control Inc | `RPC` |
| 559 | CID-00616-01, CID-00616-21 | Rubi Tools | `RTO` |
| 560 | CID-00202-01 | Rudd Lighting, Inc | `RL` |
| 561 | CID-00816-01 | S&C Electric | `SCE` |
| 562 | CID-00704-01 | Salsbury Industries | `SI` |
| 563 | CID-00500-01 | Samsung Electronics | `SE` |
| 564 | CID-00254-01 | San Jamar | `SJ` |
| 565 | CID-00401-01 | Sanmina Corporation | `SAN` |
| 566 | CID-00197-01 | Saris Cycling Group | `SCG` |
| 567 | CID-00093-01 | Sauk Technologies | `ST` |
| 568 | CID-00234-01 | Schleifring Medical Systems, LLC | `SMS` |
| 569 | CID-00308-01 | Schuette Metals Inc | `SM` |
| 570 | CID-00352-01 | Schutt Industries of Clintonville | `SIC` |
| 571 | CID-00502-01 | Scott Technologies | `STE` |
| 572 | CID-00469-01 | Screenco | `SCR` |
| 573 | CID-00034-01 | Sears Manufacturing Company | `SMA` |
| 574 | CID-00047-01 | Seats Incorporated | `SEA` |
| 575 | CID-00736-01 | Senior Flexonics - Bartlett | `SFB` |
| 576 | CID-00759-01 | Senior Metal Bellows | `SMB` |
| 577 | CID-00233-01 | Service Stampings | `SS` |
| 578 | CI-00806-01, CID-00806-01 | SEYI-America, Inc | `SA` |
| 579 | CID-00253-01 | Sharpe Products | `SP` |
| 580 | CID-00367-01 | Shoto Corporation | `SHO` |
| 581 | CID-00357-01 | Shotwell Company | `SHT` |
| 582 | CID-00095-01, CID-00185-01 | Siemens | `SIE` |
| 583 | CID-00804-01 | Simmers Crane Design & Services | `SCD` |
| 584 | CID-00588-01 | Skana Aluminum Company | `SAL` |
| 585 | CID-00240-08 | Skilled Manufacturing Inc | `SKM` |
| 586 | CID-00159-01 | SKO-DIE Inc | `SD` |
| 587 | CID-00722-01 | SMC Integration Solutions | `SIS` |
| 588 | CID-00148-01 | SMC Metal Fabricators, Inc | `SMF` |
| 589 | CID-00521-02 | Source Machine Inc | `SOM` |
| 590 | CID-00097-01, CID-00315-01 | Southbend | `SOU` |
| 591 | CID-00268-01 | Spacesaver Corporation | `SPA` |
| 592 | CID-00821-01 | SpaceX | `SPC` |
| 593 | CID-00035-01 | Spartanburg Stainless Products, Inc | `SSP` |
| 594 | CID-00491-01 | Specified Technologies, Inc | `SPT` |
| 595 | CID-00099-01 | Spindustries, LLC | `SPI` |
| 596 | CID-00250-01 | St. Marys Cement Inc | `SMC` |
| 597 | CID-00773-01 | Stainless Tank & Equipment | `STA` |
| 598 | CID-00653-01 | Stanley Black & Decker | `SBD` |
| 599 | CID-00175-01 | Star Manufacturing Company | `STM` |
| 600 | CID-00441-01 | Steel craft | `SC` |
| 601 | CID-00157-01 | Steel King Industries, Inc | `SKI` |
| 602 | CID-00238-01 | Stenograph, LLC | `STN` |
| 603 | CID-00304-01 | Stephen Braun | `SB` |
| 604 | CID-00650-01 | Sterling Pacific | `STP` |
| 605 | CID-00243-08 | Stockbridge Engineering | `SEN` |
| 606 | CID-00230-01 | Stoughton Trailers, LLC | `STR` |
| 607 | CID-00687-01 | Strato Inc | `STT` |
| 608 | CID-00292-01 | Stroh | `STO` |
| 609 | CID-00275-01 | Sullair Corporation | `SUL` |
| 610 | CID-00281-01 | Sundyne Electromagnetics | `SEL` |
| 611 | CID-00577-01 | Sunrise Bolts | `SBO` |
| 612 | CID-00531-01 | Super Radiator Coils | `SRC` |
| 613 | CID-00432-01 | Super Sky Product Enterprises, LLC | `SSK` |
| 614 | CID-00266-01 | Super Steel Corp | `SST` |
| 615 | CID-00628-01 | Superior Die Set | `SDS` |
| 616 | CID-00327-01 | Superior Windings LLC | `SW` |
| 617 | CID-00496-01 | Swift Manufacturing | `SWM` |
| 618 | CID-00633-01 | Tachi-S Engineering | `TSE` |
| 619 | CID-00819-01 | Talan Products | `TP` |
| 620 | CID-00484-01 | Taylor | `TAY` |
| 621 | CID-00802-01 | Tbsi | `TBS` |
| 622 | CID-00620-01 | Techtronic Industries Power Equipment, NA | `TIP` |
| 623 | CID-00104-01 | Telex Communications | `TC` |
| 624 | CID-00231-01 | Tella Tool & Mfg. Co | `TT` |
| 625 | CID-00036-01, CID-00036-02 | Tempel | `TMP` |
| 626 | CID-00509-01 | Tenere Inc | `TEN` |
| 627 | CID-00525-01 | Tennant Co | `TNN` |
| 628 | CID-00810-01 | Tesla | `TES` |
| 629 | CID-00703-01 | Textron | `TEX` |
| 630 | CID-00646-01 | TF-Metal | `TM` |
| 631 | CID-00244-01 | The Brewer Company | `BRE` |
| 632 | CID-00063-01 | The Cawley Company | `CAW` |
| 633 | CID-00745-01 | The Durham Company | `DUR` |
| 634 | CID-00608-01 | The Hilliard Corporation | `HIL` |
| 635 | CID-00753-01, CID-00762-01 | The Jor-Mac Company | `JMA` |
| 636 | CID-00354-01, CID-00394-01 | The Metal Ware Corporation | `MW` |
| 637 | CID-00040-01 | The Vollrath Company, LLC | `VLL` |
| 638 | CID-00450-01 | Therma-Stor LLC | `TS` |
| 639 | CID-00728-01 | Thermal Edge Inc | `TE` |
| 640 | CID-00558-01 | Thermo King | `TK` |
| 641 | CID-00768-01 | Thomas Built Buses | `TBB` |
| 642 | CID-00309-01 | Thomas La Rue & Associates, Inc | `TLR` |
| 643 | CID-00587-01 | Thor Industrial Services | `TIS` |
| 644 | CID-00037-02, CID-00463-01 | ThyssenKrupp | `THY` |
| 645 | CID-00532-01 | TitanX Engine Cooling | `TEC` |
| 646 | CID-00506-01 | Top Water Lures | `TWL` |
| 647 | CID-00178-01 | Toro Company | `TOR` |
| 648 | CID-00333-01 | Tower Tech Systems, Inc | `TTS` |
| 649 | CID-00619-01 | Toyo Automotive Parts, Inc | `TAP` |
| 650 | CID-00503-01 | Toyota Industrial Equip. MFG.Inc | `TIE` |
| 651 | CID-00388-01 | Trachte Building Systems/ Trac Rite doors | `TBU` |
| 652 | CID-00321-01 | Tramec | `TRA` |
| 653 | CID-00215-01 | Tramontina | `TRM` |
| 654 | CID-00038-01, CID-00109-02 | Trane | `TRN` |
| 655 | CID-00747-01 | Transaxle Manufacturing of America | `TMA` |
| 656 | CID-00370-01 | Traverse City Products, Inc | `TCP` |
| 657 | CID-00696-01 | TRB Lightweight Structures | `TLS` |
| 658 | CID-00721-01 | Treya Partners | `TPA` |
| 659 | CID-00542-01 | Triangle Mfg.co | `TRI` |
| 660 | CID-00332-01 | Trombetta Motion Technologies | `TMT` |
| 661 | CID-00323-01 | TS Tech Tri-Con Industries | `TTT` |
| 662 | CID-00149-01 | Tulip Corporation | `TUL` |
| 663 | CID-00111-01 | Twin Disc Incorporated | `TD` |
| 664 | CID-00317-01, CID-00505-01 | Tyco | `TYC` |
| 665 | CID-00328-01 | U.S. Custom MFG | `USC` |
| 666 | CID-00482-01 | Uelner Precision Tools and Dies | `UPT` |
| 667 | CID-00769-01 | Ultra Fit Manufacturing | `UFM` |
| 668 | CID-00715-01 | Unarco Material Handling | `UMH` |
| 669 | CID-00655-01 | Underwater Kinetics | `UK` |
| 670 | CID-00583-01 | United Alloy Inc | `UA` |
| 671 | CID-00550-01 | United Global Sourcing Inc | `UGS` |
| 672 | CID-00365-01 | USP Structural Connectors | `UST` |
| 673 | CID-00212-01 | Utility Tool & Trailer/APL, Inc | `UTT` |
| 674 | CID-00279-01 | Valeo Engine Cooling Inc | `VEC` |
| 675 | CID-00548-01 | Valley Grinding and Manufacturing | `VGM` |
| 676 | CID-00341-01 | Valley View Industries | `VVI` |
| 677 | CID-00127-01 | VanArt Engineering Company | `VE` |
| 678 | CID-00657-01 | Vehicle Security Innovators | `VSI` |
| 679 | CID-00383-01 | Venture MFG Group | `VG` |
| 680 | CID-00221-01 | VIASYSTEMS | `VIA` |
| 681 | CID-00751-01 | Viega | `VIE` |
| 682 | CID-00512-01 | Viking Electronics inc | `VEL` |
| 683 | CID-00039-01 | Vinyl-Pro Windows | `VPW` |
| 684 | CID-00671-22 | Voith Meri Environmental Solutions, Inc | `VME` |
| 685 | CID-00681-01, CID-00731-01, CID-00731-02, CID-00731-03, CID-00731-04 | Volvo | `VOL` |
| 686 | CID-00833-01 | VOXX by Gentex Corporation | `VBG` |
| 687 | CID-00713-01 | VSI, LLC | `VS` |
| 688 | CID-00668-01 | Vulcan Industries | `VI` |
| 689 | CID-00580-01 | WABCO Vehicle Control Systems | `WVC` |
| 690 | CID-00513-01 | Wald Wire and Manufacturing Co | `WWM` |
| 691 | CID-00411-01 | Watlow Hannibal | `WH` |
| 692 | CID-00453-01 | Watry Industries, LLC | `WI` |
| 693 | CID-00037-01 | Waupaca Foundry Inc | `WF` |
| 694 | CID-00638-01 | Wausau Supply Co | `WS` |
| 695 | CID-00314-01 | Weasler Engineering, Inc | `WE` |
| 696 | CID-00490-01 | Weber-Stephen Products LLC | `WSP` |
| 697 | CID-00137-01 | Weldall Mfg., Inc | `WEL` |
| 698 | CID-00705-01 | Wells | `WLL` |
| 699 | CID-00156-01 | Wells Manufacturing | `WM` |
| 700 | CID-00041-01 | Western Industries, Inc | `WIN` |
| 701 | CID-00134-01 | Whitewater Manufacturing Company | `WMA` |
| 702 | CID-00224-01 | Wichita Clutch | `WC` |
| 703 | CID-00042-01 | Williams International | `WII` |
| 704 | CID-00249-01 | Williamsburg Manufacturing | `WIM` |
| 705 | CID-00349-01, CID-00514-01 | WireTech Fabricators | `WFA` |
| 706 | CID-00169-01 | Wisco Industries, Inc | `WIS` |
| 707 | CID-00120-01 | Wisconsin Aluminum Foundry Co., Inc | `WAF` |
| 708 | CID-00527-01 | Wisconsin Automation | `WA` |
| 709 | CID-00602-01 | Wisconsin Fastener Supply Inc | `WFS` |
| 710 | CID-00139-01 | Wisconsin Metal Products Co | `WMP` |
| 711 | CID-00043-01 | Wisconsin Plastics Inc | `WP` |
| 712 | CID-00044-01 | Wisconsin Tool & Stamping Co | `WTS` |
| 713 | CID-00330-01 | Wolf Robotics | `WR` |
| 714 | CID-00376-01 | Woods Equipment Company | `WEQ` |
| 715 | CID-00551-01 | Worthington Industries | `WOI` |
| 716 | CID-00196-01 | Wrico Stamping CO. of MN | `WSM` |
| 717 | CID-00124-01 | Wrought Washer Mfg., Inc | `WW` |
| 718 | CID-00711-01 | Wstfab LLC | `WST` |
| 719 | CID-00582-01 | Xometry | `XOM` |
| 720 | CID-00257-01 | XTen Industries | `XI` |
| 721 | CID-00765-01 | Yakima Products, Inc | `YP` |
| 722 | CID-00586-01 | Yokohama Industries Americas Inc | `YIA` |
| 723 | CID-00485-01 | Zero Zone, Inc | `ZZ` |
| 724 | CID-00684-01 | ZETAGROUP Engineering | `ZE` |
| 725 | CID-00009-01, CID-00045-01, CID-00045-02, CID-00347-01, CID-00428-01, CID-00516-01, CID-00516-02 | ZF | `ZF` |
| 726 | CID-00358-11 | Zinga Industries Inc | `ZI` |

## Appendix: unnamed customer records

These records exist in the `CUSTOMER` table but have no name, so they are excluded from the acronym key above.

| Customer ID | Active | City | State |
| --- | --- | --- | --- |
| CID-00356-01 | Y |  |  |
| CID-00463 | Y |  | AK |
| CID-0484-01 | Y |  | AK |
| CID-0485-01 | Y |  | IA |
| CID-0486-01 | Y |  | AZ |
| CID-0487-01 | N |  | WI |
| CID00338-01 | Y |  |  |
| CID=00516-01 | Y |  |  |

## Companies collapsed from multiple customer IDs

These companies appear under more than one Infor Visual customer ID and were merged into a single entry above.

| Company | Acronym | Customer IDs |
| --- | --- | --- |
| Aisin USA Mfg., Inc | `AIS` | 2 |
| Apache Stainless Equipment Corporation | `ASE` | 2 |
| Bosch | `BOS` | 3 |
| Bradley Corp | `BRA` | 2 |
| Broan-NuTone | `BNU` | 2 |
| Brunner | `BRU` | 2 |
| Cardinal Components Inc | `CC` | 2 |
| Carrier | `CAR` | 3 |
| Caterpillar | `CAT` | 2 |
| Continental Industries Inc | `CIN` | 2 |
| Cretex Specialty Products | `CSP` | 2 |
| Daimler Trucks North America | `DTN` | 33 |
| Detroit Diesel | `DD` | 2 |
| Discover Energy | `DSE` | 2 |
| Donaldson | `DON` | 2 |
| DR Power | `DPO` | 2 |
| FASTENAL Company | `FAS` | 2 |
| Faurecia | `FAU` | 2 |
| Fawn Manufacturing | `FM` | 2 |
| Federal-Mogul | `FED` | 3 |
| Fisher Barton | `FIB` | 3 |
| Generac | `GEN` | 3 |
| Grede Foundries, Inc | `GF` | 2 |
| Harper Trucks, Inc | `HT` | 2 |
| Heraeus Electro-Nite | `HEN` | 3 |
| IEA | `IEA` | 2 |
| IMMI | `IMM` | 3 |
| John Deere | `JD` | 20 |
| Kawasaki Motors Mfg. Corp | `KM` | 3 |
| Kees | `KEE` | 2 |
| Kohler | `KOH` | 5 |
| Krueger International | `KI` | 4 |
| Kruger | `KRU` | 2 |
| Lake Shore Systems | `LSS` | 2 |
| LDI Industries | `LDI` | 2 |
| Lennox | `LEN` | 3 |
| Lippert | `LIP` | 2 |
| MAHLE | `MAH` | 3 |
| Marion Body Works, Inc | `MBW` | 2 |
| Metals USA | `MES` | 2 |
| Miller Electric | `MLE` | 2 |
| Milwaukee Tool | `MIL` | 2 |
| Modern Tool Inc | `MTO` | 2 |
| Molded Fiber Glass | `MFG` | 2 |
| National Metalwares, LP | `NM` | 2 |
| Nemak | `NEM` | 2 |
| NN Inc | `NN` | 2 |
| Northport / MirroCraft | `NMI` | 2 |
| NTN | `NTN` | 2 |
| Orion Energy Systems | `OES` | 2 |
| P&P Industries | `PPI` | 2 |
| Parker Hannifin | `PAR` | 3 |
| Pentair | `PNT` | 4 |
| Prestige Threaded Products | `PTP` | 2 |
| Professional Power Products | `PPP` | 2 |
| Rafael | `RAF` | 3 |
| Regal Beloit | `RB` | 3 |
| RES Manufacturing Co | `RM` | 2 |
| Rubi Tools | `RTO` | 2 |
| SEYI-America, Inc | `SA` | 2 |
| Siemens | `SIE` | 2 |
| Southbend | `SOU` | 2 |
| Tempel | `TMP` | 2 |
| The Jor-Mac Company | `JMA` | 2 |
| The Metal Ware Corporation | `MW` | 2 |
| ThyssenKrupp | `THY` | 2 |
| Trane | `TRN` | 2 |
| Tyco | `TYC` | 2 |
| Volvo | `VOL` | 5 |
| WireTech Fabricators | `WFA` | 2 |
| ZF | `ZF` | 7 |

## Notes

- Acronyms are assigned automatically from the company name (word initials, then leading letters) and are guaranteed unique.
- Inactive (`ACTIVE_FLAG = N`) customers are included because they still appear in historical transactions.
- Groups were collapsed only when the names clearly describe the same company. Distinct businesses that merely share a word (for example Manitowoc Cranes vs Manitowoc Heating) remain separate.
