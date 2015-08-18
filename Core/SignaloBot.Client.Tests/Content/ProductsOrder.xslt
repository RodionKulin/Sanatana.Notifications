<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<html>
			<body>
				<xsl:variable name="orderId" select="mailData/@orderId"/>
				<xsl:variable name="totalSum" select="sum(mailData/orderedProducts/orderedProduct/price)"/>
				<p>
					Dear <xsl:value-of select="mailData/name"/>,
				</p>
				<p>
					On <xsl:value-of select="mailData/orderDate"/> you ordered some product from 
					<a href="http://www.ourwebsite.com/">our website</a>.
				</p>
				<ul>
					<xsl:for-each select="mailData/orderedProducts/orderedProduct">
						<li>
							Order <xsl:value-of select="$orderId"/>:
							<a>
								<xsl:attribute name="href">
									http://www.ourwebsite.com/productInfo.aspx?productId=
									<xsl:value-of select="@id"/>
								</xsl:attribute>
								<xsl:value-of select="name"/>
							</a> - $<xsl:value-of select="format-number(price, '#.00')"/>
							<br/>
						</li>
					</xsl:for-each>
				</ul>
				Total price: $<xsl:value-of select="format-number($totalSum, '#.00')"/>
				<xsl:if test="count(mailData/orderedProducts/orderedProduct) &gt; 2">
					<p>
						You ordered more than 2 products and you will receive a <b>free hat</b>.
					</p>
				</xsl:if>
				<xsl:if test="$totalSum &gt; 20">
					<p>
						You ordered products for more than $<xsl:value-of select="format-number(20, '#.00')"/>
						and the delivery of the products will be <b>free</b>.
					</p>
				</xsl:if>
				<p>
					To confirm your order please follow 
					<a>
						<xsl:attribute name="href">
							http://www.ourwebsite.com/orders.aspx?orderId=
							<xsl:value-of select="$orderId"/>
							&amp;customerName=
							<xsl:value-of select="mailData/name"/>
						</xsl:attribute>
						this link
					</a>.
				</p>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>