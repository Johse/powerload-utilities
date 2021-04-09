<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" 
		xmlns:h="http://schemas.autodesk.com/pseb/dm/DataImport/2015-04-14">
	<xsl:output method="text" indent="no"/>	
	<xsl:param name="documentName"></xsl:param>
	<xsl:variable name="delimiter" select="';'"/>
	<xsl:variable name="list" select="document($documentName)/list/UDP"/>
	<xsl:variable name="lowercase">abcdefghijklmnopqrstuvwxyz</xsl:variable>
	<xsl:variable name="uppercase">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
	<xsl:template match="/">
		<xsl:text>LocalFullFileName;FileID;FolderID;FileName;Category;Classification;RevisionLabel;RevisionDefinition;Version;LifecycleState;LifecycleDefinition;Comment;CreateUser;CreateDate;IterationID;Path</xsl:text>		
		<xsl:for-each select="$list">
			<xsl:value-of select="$delimiter" />
			<xsl:value-of select="." />
		</xsl:for-each>
		<xsl:text>&#10;</xsl:text>
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="h:File">
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:Revision">
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	<xsl:template match="text()"/>	
	<xsl:template match="h:Folder/h:UDP"/>
	<xsl:template match="h:Iteration">
		<xsl:variable name="col" select="."/>
		<xsl:value-of select="concat(@LocalPath,$delimiter,$delimiter,0,$delimiter,ancestor::h:File/@Name, $delimiter, ancestor::h:File/@Category, $delimiter, ancestor::h:File/@Classification, $delimiter, ancestor::h:Revision/@Label, $delimiter, ancestor::h:Revision/@Definition, $delimiter)"/>
		<xsl:number/>
		<xsl:value-of select="$delimiter" />
		<xsl:value-of select="concat(h:State/@Name, $delimiter, h:State/@Definition, $delimiter, @Comment, $delimiter, h:Created/@User, $delimiter, h:Created/@Date, $delimiter, substring(@Id,2))"/>
		<xsl:value-of select="$delimiter" />
		<xsl:text>$</xsl:text>
		<xsl:apply-templates select="ancestor-or-self::h:Folder/@Name"/>
		<xsl:variable name="file" select="."/>
		<xsl:for-each select="$list">
			<xsl:value-of select="$delimiter" />
			<xsl:variable name="udpName" select="."/>
			<xsl:variable name="datatype" select="@DataType"/>
			<xsl:for-each select="$file/h:UDP">
				<xsl:variable name="Name" select="translate(@Name, ' ','_')"/>
				<xsl:if test="concat('UDP_',$Name) = $udpName">
					<xsl:choose>
						<xsl:when test="($datatype='bit') and ((translate(.,$uppercase,$lowercase)='true'))">1</xsl:when>
						<xsl:when test="($datatype='bit') and ((translate(.,$uppercase,$lowercase)='false'))">0</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select=" normalize-space(.)"/>
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
</xsl:stylesheet>
