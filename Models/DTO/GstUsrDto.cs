namespace Models.DTO;

public class GstUsrInfoDbDto
{
    public int NrSeededFriends { get; set; }
    public int NrUnseededFriends { get; set; }
    public int NrFriendsWithAddress { get; set; }

    public int NrSeededAddresses { get; set; }
    public int NrUnseededAddresses { get; set; }

    public int NrSeededPets { get; set; }
    public int NrUnseededPets { get; set; }

    public int NrSeededQuotes { get; set; }
    public int NrUnseededQuotes { get; set; }
}

public class GstUsrInfoFriendsDto
{
    public string Country { get; set; }
    public string City { get; set; }
    public int NrFriends { get; set; }
}

public class GstUsrInfoPetsDto
{
    public string Country { get; set; }
    public string City { get; set; }
    public int NrPets { get; set; }
}

public class GstUsrInfoQuotesDto
{
    public string Author { get; set; }
    public int NrQuotes { get; set; }
}

public class GstUsrInfoAllDto
{
    public GstUsrInfoDbDto Db { get; set; }
    public List<GstUsrInfoFriendsDto> Friends { get; set; }
    public List<GstUsrInfoPetsDto> Pets { get; set; }
    public List<GstUsrInfoQuotesDto> Quotes { get; set; }
}


