<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0"
    xmlns:h="http://schemas.autodesk.com/pseb/dm/DataImport/2015-04-14">
  <xsl:output method="text" indent="no"/>
	<xsl:param name="documentName"></xsl:param>
	<xsl:variable name="delimiter" select="';'"/>
	<xsl:variable name="list" select="document($documentName)/list/UDP"/>
	<xsl:variable name="lowercase">abcdefghijklmnopqrstuvwxyz</xsl:variable>
	<xsl:variable name="uppercase">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
  <xsl:template match="/">
	<xsl:text>FolderID;ParentFolderId;FolderName;Path;IsLibrary;LifeCycleState;LifeCycleDefinition;Category;User;CreateDate</xsl:text>
	<xsl:for-each select="$list">
			<xsl:value-of select="$delimiter" />
			<xsl:value-of select="." />
		</xsl:for-each>
      <xsl:text>&#10;</xsl:text>
      <xsl:apply-templates select="//h:Folder" />
  </xsl:template>  
  <xsl:template match="h:Folder">
  <xsl:value-of select="concat($delimiter, $delimiter, @Name)"/>
  <xsl:value-of select="$delimiter" />
  <xsl:text>$</xsl:text>
  <xsl:variable name="curr" select="."/>
	<xsl:apply-templates select="ancestor-or-self::h:Folder/@Name"/>
	<xsl:value-of select="$delimiter" />
	<xsl:choose>
		<xsl:when test="translate(@IsLibrary,$uppercase,$lowercase)='true'">1</xsl:when>
		<xsl:otherwise>
			<xsl:text>0</xsl:text>
		</xsl:otherwise>
	</xsl:choose>
	<xsl:value-of select="concat($delimiter,$delimiter,$delimiter, @Category, $delimiter, h:Created/@User, $delimiter, h:Created/@Date)"/>
	<xsl:for-each select="$list">		
		<xsl:variable name="udpName" select="."/>
		<xsl:variable name="datatype" select="@DataType"/>
		<xsl:value-of select="$delimiter" />
		<xsl:for-each select="$curr/h:UDP">
			<xsl:variable name="Name" select="translate(@Name, ' ','_')"/>
			<xsl:if test="concat('UDP_',$Name) = $udpName">
				<xsl:choose>
					<xsl:when test="($datatype='bit') and ((translate(.,$uppercase,$lowercase)='true'))">1</xsl:when>
					<xsl:when test="($datatype='bit') and ((translate(.,$uppercase,$lowercase)='false'))">0</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="normalize-space(.)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
	</xsl:for-each>
      <xsl:text>&#10;</xsl:text>
  </xsl:template>
    
  <xsl:template match="h:Folder/@Name">
      <xsl:if test="position() > 0">/</xsl:if>
      <xsl:value-of select="."/>
  </xsl:template> 
  	<xsl:template match="text()"/>	
</xsl:stylesheet>
