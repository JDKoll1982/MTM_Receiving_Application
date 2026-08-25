SET
    FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS dunnage_custom_field_choices;

SET
    FOREIGN_KEY_CHECKS = 1;

CREATE TABLE IF NOT EXISTS dunnage_custom_field_choices (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each choice',
    CustomFieldID INT NOT NULL COMMENT 'References dunnage_custom_fields.ID',
    Choice VARCHAR(255) NOT NULL COMMENT 'Display value of the choice',
    SortOrder INT NOT NULL COMMENT 'Display order of the choice within the field',
    UNIQUE KEY IDX_UDC_CHOICES_001 (CustomFieldID, Choice) COMMENT 'No duplicate choices per field',
    KEY IDX_UDC_CHOICES_002 (CustomFieldID) COMMENT 'FK lookup performance',
    CONSTRAINT FK_UDC_CHOICES_FIELD FOREIGN KEY (CustomFieldID)
        REFERENCES dunnage_custom_fields (ID) ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Choice list options for dunnage custom fields of type Choices';
