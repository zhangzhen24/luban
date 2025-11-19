/**
 * 配置管理器
 * 自动生成文件，请勿手动修改
 */
class UCfgMgr : UScriptGameInstanceSubsystem {
	UPROPERTY(BlueprintReadOnly, Category = "Config")
	TMap<int32, FCreateCharacterCfg> CreateCharacterCfgMap;

	UPROPERTY(BlueprintReadOnly, Category = "Config")
	TMap<int32, FI18NCfg> I18NCfgMap;

	UPROPERTY(BlueprintReadOnly, Category = "Config")
	TMap<int32, FItemCfg> ItemCfgMap;

	UPROPERTY(BlueprintReadOnly, Category = "Config")
	TMap<int32, FItemRarityColorCfg> ItemRarityColorCfgMap;

	UPROPERTY(BlueprintReadOnly, Category = "Config")
	TMap<int32, FTestCharacterCfg> TestCharacterCfgMap;

	UFUNCTION(BlueprintOverride)
	void Initialize() {
		CreateCharacterCfgMap.Empty();
		I18NCfgMap.Empty();
		ItemCfgMap.Empty();
		ItemRarityColorCfgMap.Empty();
		TestCharacterCfgMap.Empty();
		LoadConfigs();
	}

	UFUNCTION(BlueprintOverride)
	void Deinitialize() {
	}

	UFUNCTION()
	void LoadConfigs() {
		FString JsonDirectory = FPaths::CombinePaths(FPaths::ProjectConfigDir(), "gen");
		if (!FPaths::DirectoryExists(JsonDirectory)) {
			Error("[CfgMgr] JsonDirectory not found: " + JsonDirectory);
			return;
		}

		TArray<FString> JsonFiles;
		if (!NamiFilePath::FindFiles(JsonFiles, JsonDirectory, "*.json", true, true, true)) {
			Error("[CfgMgr] ListDirectory failed: " + JsonDirectory);
			return;
		}

		for (FString JsonFile : JsonFiles) {
			ParseJsonFile(JsonFile);
		}
	}

	UFUNCTION(BlueprintPure)
	bool GetCreateCharacterCfg(int32 ID, FCreateCharacterCfg&out CreateCharacterCfg) {
		if (CreateCharacterCfgMap.Contains(ID)) {
			CreateCharacterCfg = CreateCharacterCfgMap[ID];
			return true;
		}
		return false;
	}

	UFUNCTION(BlueprintPure)
	bool GetI18NCfg(int32 ID, FI18NCfg&out I18NCfg) {
		if (I18NCfgMap.Contains(ID)) {
			I18NCfg = I18NCfgMap[ID];
			return true;
		}
		return false;
	}

	UFUNCTION(BlueprintPure)
	bool GetItemCfg(int32 ID, FItemCfg&out ItemCfg) {
		if (ItemCfgMap.Contains(ID)) {
			ItemCfg = ItemCfgMap[ID];
			return true;
		}
		return false;
	}

	UFUNCTION(BlueprintPure)
	bool GetItemRarityColorCfg(int32 ID, FItemRarityColorCfg&out ItemRarityColorCfg) {
		if (ItemRarityColorCfgMap.Contains(ID)) {
			ItemRarityColorCfg = ItemRarityColorCfgMap[ID];
			return true;
		}
		return false;
	}

	UFUNCTION(BlueprintPure)
	bool GetTestCharacterCfg(int32 ID, FTestCharacterCfg&out TestCharacterCfg) {
		if (TestCharacterCfgMap.Contains(ID)) {
			TestCharacterCfg = TestCharacterCfgMap[ID];
			return true;
		}
		return false;
	}

	UFUNCTION()
	private void ParseJsonFile(FString JsonFile) {
		FString JsonContent;
		if (!NamiFilePath::LoadFileToString(JsonFile, JsonContent)) {
			Error("[CfgMgr] Failed to load file: " + JsonFile);
			return;
		}

		// 根据文件名判断配置类型
		FString FileName = FPaths::GetBaseFilename(JsonFile);
		if (FileName.Contains("CreateCharacterCfg")) {
			FCreateCharacterCfg CreateCharacterCfg;
			TArray<FString> JsonKeys;
			TArray<FString> JsonValues;
			if (NamiJson::JsonParseObjectFields(JsonContent, JsonKeys, JsonValues)) {
				for (int32 i = 0; i < JsonKeys.Num(); i++) {
					const FString& K = JsonKeys[i];
					const FString& V = JsonValues[i];
					if (FJsonObjectConverter::JsonObjectStringToUStruct(V, CreateCharacterCfg)) {
						int32 ID = String::Conv_StringToInt(K);
						CreateCharacterCfg.ID = ID;
						CreateCharacterCfgMap.Add(ID, CreateCharacterCfg);
					}
				}
			}
		}
		else if (FileName.Contains("I18NCfg")) {
			FI18NCfg I18NCfg;
			TArray<FString> JsonKeys;
			TArray<FString> JsonValues;
			if (NamiJson::JsonParseObjectFields(JsonContent, JsonKeys, JsonValues)) {
				for (int32 i = 0; i < JsonKeys.Num(); i++) {
					const FString& K = JsonKeys[i];
					const FString& V = JsonValues[i];
					if (FJsonObjectConverter::JsonObjectStringToUStruct(V, I18NCfg)) {
						int32 ID = String::Conv_StringToInt(K);
						I18NCfg.ID = ID;
						I18NCfgMap.Add(ID, I18NCfg);
					}
				}
			}
		}
		else if (FileName.Contains("ItemCfg")) {
			FItemCfg ItemCfg;
			TArray<FString> JsonKeys;
			TArray<FString> JsonValues;
			if (NamiJson::JsonParseObjectFields(JsonContent, JsonKeys, JsonValues)) {
				for (int32 i = 0; i < JsonKeys.Num(); i++) {
					const FString& K = JsonKeys[i];
					const FString& V = JsonValues[i];
					if (FJsonObjectConverter::JsonObjectStringToUStruct(V, ItemCfg)) {
						int32 ID = String::Conv_StringToInt(K);
						ItemCfg.ID = ID;
						ItemCfgMap.Add(ID, ItemCfg);
					}
				}
			}
		}
		else if (FileName.Contains("ItemRarityColorCfg")) {
			FItemRarityColorCfg ItemRarityColorCfg;
			TArray<FString> JsonKeys;
			TArray<FString> JsonValues;
			if (NamiJson::JsonParseObjectFields(JsonContent, JsonKeys, JsonValues)) {
				for (int32 i = 0; i < JsonKeys.Num(); i++) {
					const FString& K = JsonKeys[i];
					const FString& V = JsonValues[i];
					if (FJsonObjectConverter::JsonObjectStringToUStruct(V, ItemRarityColorCfg)) {
						int32 ID = String::Conv_StringToInt(K);
						ItemRarityColorCfg.ID = ID;
						ItemRarityColorCfgMap.Add(ID, ItemRarityColorCfg);
					}
				}
			}
		}
		else if (FileName.Contains("TestCharacterCfg")) {
			FTestCharacterCfg TestCharacterCfg;
			TArray<FString> JsonKeys;
			TArray<FString> JsonValues;
			if (NamiJson::JsonParseObjectFields(JsonContent, JsonKeys, JsonValues)) {
				for (int32 i = 0; i < JsonKeys.Num(); i++) {
					const FString& K = JsonKeys[i];
					const FString& V = JsonValues[i];
					if (FJsonObjectConverter::JsonObjectStringToUStruct(V, TestCharacterCfg)) {
						int32 ID = String::Conv_StringToInt(K);
						TestCharacterCfg.ID = ID;
						TestCharacterCfgMap.Add(ID, TestCharacterCfg);
					}
				}
			}
		}
		else {
			Warning("[CfgMgr] Unknown config file type: " + FileName);
		}
	}
}
